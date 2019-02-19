using System;
using System.Collections.Generic;
using System.Text;

namespace OneOne.Core.Cache.Couchbase
{
    using System.Collections.Concurrent;
    using System.IO;
    using System.IO.Compression;
    using System.Threading;

    using global::Couchbase;
    using global::Couchbase.Authentication;
    using global::Couchbase.Configuration.Client;
    using global::Couchbase.Core;

    using ProtoBuf;

    using OneOne.Core.Cache.Common;
    using OneOne.Core.Logger;

    public class CouchbaseManager : IDisposable
    {
        private static Cluster _instance;
        private static ConcurrentDictionary<string, ThreadQueue> _keyThreadQueue = new ConcurrentDictionary<string, ThreadQueue>();
        private readonly static object _keyThreadQueueLock = new object();

        private static readonly TaskPool _getCASTaskPool = new TaskPool(ThreadPriority.AboveNormal, 2, 10);

        public CouchbaseManager(ClientConfiguration clientConfiguration, ClusterCredentials clusterCredentials)
        {
            if (_instance == null)
            {
                _instance = new Cluster(clientConfiguration);
                _instance.Authenticate(clusterCredentials);
            }
        }

        ~CouchbaseManager()
        {
            Dispose(false);
        }

        private Cluster Instance { get { return _instance; } }

        #region IDisposable 成员

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // _keyThreadQueue = null;
            }

            if (_instance != null)
            {
                _instance.Dispose();
            }
        }

        public Cluster GetCluster()
        {
            return _instance;
        }

        #endregion

        /// <summary>
        /// 获取bucket对象
        /// </summary>
        /// <param name="bucketName">bucket名称</param>
        /// <returns></returns>
        public IBucket GetBucket(string bucketName)
        {
            return this.OpenBucket(bucketName);
        }

        /// <summary>
        /// 添加缓存(未指定过期时间，则永不过期)
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">键值</param>
        /// <param name="bucketName">bucket名称</param>
        public bool Add<T>(string key, T value, string bucketName)
        {
            return Update(key, value, bucketName);
        }

        /// <summary>
        /// 添加缓存(指定缓存绝对过期时间)
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">键值</param>
        /// <param name="numOfMinutes">缓存绝对过期时间值(分钟计)</param>
        /// <param name="bucketName">bucket名称</param>
        public bool Add<T>(string key, T value, long numOfMinutes, string bucketName)
        {
            return Update(key, value, numOfMinutes, bucketName);
        }

        /// <summary>
        /// 更新指定缓存
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">键值</param>
        /// <param name="bucketName">bucket名称</param>
        /// <returns></returns>
        public bool Update(string key, object value, string bucketName)
        {
            return Update(key, value, -1, bucketName);
        }

        /// <summary>
        /// 更新指定缓存，有过期时间
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">键值</param>
        /// <param name="numOfMinutes">缓存时间，分钟计</param>
        /// <param name="bucketName">bucket名称</param>
        /// <returns></returns>
        public bool Update(string key, object value, long numOfMinutes, string bucketName)
        {
            IOperationResult<byte[]> result = null;
            var bucket = this.GetBucket(bucketName);
            if (numOfMinutes < 0)
            {
                result = bucket.Upsert(key, GetBytes(value));
            }
            else
            {
                var timeSpan = new TimeSpan(0, (int)numOfMinutes, 0);
                result = bucket.Upsert(key, GetBytes(value), timeSpan);
            }

            if (!result.Success)
            {
                if (result.Exception != null)
                    throw result.Exception;
                throw new Exception(String.Format("Couchbase Error Code: {0},Message: {1}", result.Status, result.Message));
            }
            return true;
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key">couchbase缓存键</param>
        /// <param name="bucketName">bucket名称</param>
        /// <returns>true 移除成功，false 移除失败</returns>
        /// <exception cref="System.ArgumentNullException">在参数key值为null时抛出</exception>
        public bool Remove(string key, string bucketName)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            var bucket = this.GetBucket(bucketName);
            if (!bucket.Exists(key))
            {
                return true;
            }
            return bucket.Remove(key).Success;
        }

        /// <summary>
        /// 获取Cache值对象
        /// 
        /// 需要兼容以前的以MemoryStream存储的格式
        /// </summary>
        /// <param name="key">要获取的Cache项键名</param>
        /// <param name="bucketName">bucket名称</param>
        /// <returns>Cache对象</returns>
        public T Get<T>(string key, string bucketName) where T : class
        {
            var bucket = this.GetBucket(bucketName);
            var obj = bucket.Get<byte[]>(key);

            if (obj == null)
                return null;

            var result = Deserialize<T>(obj.Value);
            return result;
        }

        /// <summary>
        /// 更新缓存，该缓存没有失效时间<br/>
        /// 已实现分布式锁，保证在缓存更新过程中不会被有其他线程更新<br/>
        /// 适用场景：当更新的对象正处于被其他线程取出计算后更新过程时，等待该线程更新完成之后覆盖
        /// </summary>
        /// <typeparam name="T">更新缓存对象类型</typeparam>
        /// <param name="key">couchbase缓存key</param>
        /// <param name="value">couchbase缓存对象 </param>
        /// <param name="lockTimeOut">获取锁等待超时时间，超过该时间仍不能获取到锁，抛出异常<br/> 单位：毫秒</param>
        /// <exception cref="System.ArgumentNullException">在参数key值为null时抛出</exception>
        /// <exception cref="System.ArgumentException">在参数lockTimeOut小于0时抛出</exception>
        /// <exception cref="System.TimeoutException">在lockTimeOut时间之后仍然无法获取到couchbase锁时抛出</exception>
        public void UpdateWithLock<T>(string key, T value, string bucketName, int lockTimeOut = 3 * 1000)
        {
            UpdateWithLock(key, value, -1, lockTimeOut, bucketName);
        }

        /// <summary>
        /// 更新缓存，指定过期时间后失效<br/>
        /// 已实现分布式锁，保证在缓存更新过程中不会被有其他线程更新<br/>
        /// 适用场景：当更新的对象正处于被其他线程取出计算后更新过程时，等待该线程更新完成之后覆盖
        /// </summary>
        /// <typeparam name="T">更新缓存对象类型</typeparam>
        /// <param name="key">couchbase缓存key</param>
        /// <param name="value">couchbase缓存对象 </param>
        /// <param name="cacheNumOfMinutes">缓存对象失效时间，小于0则没有永久存储<br/>单位：分钟</param>
        /// <param name="lockTimeOut">获取锁等待超时时间，超过该时间仍不能获取到锁，抛出异常<br/> 单位：ms</param>
        /// <param name="bucketName">bucket名称</param>
        /// <exception cref="System.ArgumentNullException">在参数key值为null时抛出</exception>
        /// <exception cref="System.ArgumentException">在参数lockTimeOut小于0时抛出</exception>
        /// <exception cref="System.TimeoutException">在lockTimeOut时间之后仍然无法获取到couchbase锁时抛出</exception>
        public void UpdateWithLock<T>(string key, T value, int cacheNumOfMinutes, int lockTimeOut, string bucketName)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (lockTimeOut < 0)
                throw new ArgumentException("timeOut less than zero");

            Action<IOperationResult<object>> callback = lockResult =>
            {
                UpdateCas(key, value, cacheNumOfMinutes, lockResult.Cas, bucketName);
            };
            GetWithLock(key, lockTimeOut, callback, bucketName);
        }

        /// <summary>
        /// 更新缓存，该缓存没有失效时间<br/>
        /// 在当前缓存对象上执行累加后并更新原有缓存<br/>
        /// 当前缓存对象不存在或为null，使用增量直接更新缓存并返回<br/>
        /// 已实现分布式锁，保证在缓存更新过程中不会被有其他线程更新<br/>
        /// 适用场景：当需要对缓存对象（对象多个属性）做累加或修改并且需要保证修改过程中不会被其他线程更新时，使用该方法可保证不会出现“更新丢失”现象
        /// </summary>
        /// <typeparam name="T">更新缓存对象类型，该类型必须实现IAccumulator接口</typeparam>
        /// <param name="key">couchbase缓存key</param>
        /// <param name="increment">当前缓存对象增量<br/>
        /// key存在，则在该缓存对象上执行增量累加<br/>
        /// 不存在则将增量作为初始值缓存
        /// </param>
        /// <param name="bucketName">bucket名称</param>
        /// <param name="lockTimeOut">获取锁等待超时时间，超过该时间仍不能获取到锁，抛出异常<br/> 单位：毫秒</param>
        /// <returns>执行完累加后的缓存对象</returns>
        /// <exception cref="System.ArgumentNullException">在参数key或者increment值为null时抛出</exception>
        /// <exception cref="System.ArgumentException">在参数lockTimeOut小于0时抛出</exception>
        /// <exception cref="System.TimeoutException">在lockTimeOut时间之后仍然无法获取到couchbase锁时抛出</exception>
        public T IncrementWithLock<T>(string key, T increment, string bucketName, int lockTimeOut = 3 * 1000) where T : class, IAccumulator
        {
            return IncrementWithLock(key, increment, -1, lockTimeOut, bucketName);
        }

        /// <summary>
        /// 更新缓存，指定过期时间后失效<br/>
        /// 在当前缓存对象上执行累加后并更新原有缓存<br/>
        /// 当前缓存对象不存在或为null，使用增量直接更新缓存并返回<br/>
        /// 已实现分布式锁，保证在缓存更新过程中不会被有其他线程更新<br/>
        /// 适用场景：当需要对缓存对象（对象多个属性）做累加或修改并且需要保证修改过程中不会被其他线程更新时，使用该方法可保证不会出现“更新丢失”现象
        /// </summary>
        /// <typeparam name="T">更新缓存对象类型，该类型必须实现IAccumulator接口</typeparam>
        /// <param name="key">couchbase缓存key</param>
        /// <param name="increment">当前缓存对象增量<br/>
        /// key存在，则在该缓存对象上执行增量累加<br/>
        /// 不存在则将增量作为初始值缓存
        /// </param>
        /// <param name="cacheNumOfMinutes">缓存对象失效时间，小于0则没有永久存储<br/>单位：分钟</param>
        /// <param name="lockTimeOut">获取锁等待超时时间，超过该时间仍不能获取到锁，抛出异常<br/> 单位：毫秒</param>
        /// <param name="bucketName">bucket名称</param>
        /// <returns>执行完累加后的缓存对象</returns>
        /// <exception cref="System.ArgumentNullException">在参数key或者increment值为null时抛出</exception>
        /// <exception cref="System.ArgumentException">在参数lockTimeOut小于0时抛出</exception>
        /// <exception cref="System.TimeoutException">在lockTimeOut时间之后仍然无法获取到couchbase锁时抛出</exception>
        public T IncrementWithLock<T>(string key, T increment, int cacheNumOfMinutes, int lockTimeOut, string bucketName) where T : class, IAccumulator
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (increment == null)
                throw new ArgumentNullException("increment");
            if (lockTimeOut < 0)
                throw new ArgumentException("timeOut less than zero");

            T result = default(T);
            Action<IOperationResult<object>> callback = lockResult =>
            {
                if (lockResult.Value == null)
                {
                    UpdateCas(key, increment, cacheNumOfMinutes, lockResult.Cas, bucketName);
                    result = increment;
                    return;
                }

                var obj = lockResult.Value;
                T old = Deserialize<T>(obj);
                old.Add(increment);
                UpdateCas(key, old, cacheNumOfMinutes, lockResult.Cas, bucketName);
                result = old;
            };

            GetWithLock(key, lockTimeOut, callback, bucketName);
            return result;
        }

        /// <summary>
        /// 更新缓存，该缓存没有失效时间<br/>
        /// 在当前缓存对象上执行累加后并更新原有缓存<br/>
        /// 已实现分布式锁，保证在缓存更新过程中不会被有其他线程更新<br/>
        /// 适用场景：当需要对缓存对象（对象某个属性）做累加或修改并且需要保证修改过程中不会被其他线程更新时，使用该方法可保证不会出现“更新丢失”现象
        /// </summary>
        /// <typeparam name="T">更新缓存对象类型，该类型必须实现IAccumulator[TIncrement]接口，具有默认构造函数</typeparam>
        /// <typeparam name="TIncrement">被累加增量的类型</typeparam>
        /// <param name="key">couchbase缓存key</param>
        /// <param name="increment">当前缓存对象增量<br/>
        /// key存在，则在该缓存对象上执行增量累加<br/>
        /// 不存在则将增量作为初始值缓存
        /// </param>
        /// <param name="lockTimeOut">获取锁等待超时时间，超过该时间仍不能获取到锁，抛出异常<br/> 单位：毫秒</param>
        /// <returns>执行完累加后的缓存对象</returns>
        /// <exception cref="System.ArgumentNullException">在参数key值为null时抛出</exception>
        /// <exception cref="System.ArgumentException">在参数lockTimeOut小于0时抛出</exception>
        /// <exception cref="System.TimeoutException">在lockTimeOut时间之后仍然无法获取到couchbase锁时抛出</exception>
        public T IncrementWithLock<T, TIncrement>(string key, TIncrement increment, string bucketName, int lockTimeOut = 3 * 1000) where T : class, IAccumulator<TIncrement>, new()
        {
            return IncrementWithLock<T, TIncrement>(key, increment, -1, lockTimeOut, bucketName);
        }

        /// <summary>
        /// 更新缓存，指定过期时间后失效<br/>
        /// 在当前缓存对象上执行累加后并更新原有缓存<br/>
        /// 已实现分布式锁，保证在缓存更新过程中不会被有其他线程更新<br/>
        /// 适用场景：当需要对缓存对象（对象某个属性）做累加或修改并且需要保证修改过程中不会被其他线程更新时，使用该方法可保证不会出现“更新丢失”现象
        /// </summary>
        /// <typeparam name="T">更新缓存对象类型，该类型必须实现IAccumulator[TIncrement]接口，具有默认构造函数</typeparam>
        /// <typeparam name="TIncrement">被累加增量的类型</typeparam>
        /// <param name="key">couchbase缓存key</param>
        /// <param name="increment">当前缓存对象增量<br/>
        /// key存在，则在该缓存对象上执行增量累加<br/>
        /// 不存在则将增量作为初始值缓存
        /// </param>
        /// <param name="cacheNumOfMinutes">缓存对象失效时间，小于0则没有永久存储<br/>单位：分钟</param>
        /// <param name="lockTimeOut">获取锁等待超时时间，超过该时间仍不能获取到锁，抛出异常<br/> 单位：毫秒</param>
        /// <param name="bucketName"></param>
        /// <returns>执行完累加后的缓存对象</returns>
        /// <exception cref="System.ArgumentNullException">在参数key值为null时抛出</exception>
        /// <exception cref="System.ArgumentException">在参数lockTimeOut小于0时抛出</exception>
        /// <exception cref="System.TimeoutException">在lockTimeOut时间之后仍然无法获取到couchbase锁时抛出</exception>
        public T IncrementWithLock<T, TIncrement>(string key, TIncrement increment, int cacheNumOfMinutes, int lockTimeOut, string bucketName) where T : class, IAccumulator<TIncrement>, new()
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (lockTimeOut < 0)
                throw new ArgumentException("timeOut less than zero");

            T old = default(T);
            Action<IOperationResult<object>> callback = lockResult =>
            {
                if (lockResult.Value == null)
                {
                    old = new T();
                }
                else
                {
                    var obj = lockResult.Value;
                    old = Deserialize<T>(obj);
                }

                old.Add(increment);
                UpdateCas(key, old, cacheNumOfMinutes, lockResult.Cas, bucketName);
            };

            GetWithLock(key, lockTimeOut, callback, bucketName);

            return old;
        }

        /// <summary>
        /// 更新缓存，指定过期时间后失效<br/>
        /// 在当前缓存对象上执行减量后并更新原有缓存<br/>
        /// 当前缓存对象不存在或为null，不做处理，直接返回null<br/>
        /// 已实现分布式锁，保证在缓存更新过程中不会被有其他线程更新<br/>
        /// 适用场景：当需要对缓存对象（对象多个属性）做减量或修改并且需要保证修改过程中不会被其他线程更新时，使用该方法可保证不会出现“更新丢失”现象
        /// </summary>
        /// <typeparam name="T">更新缓存对象类型，该类型必须实现IDecrementer接口</typeparam>
        /// <param name="key">couchbase缓存key</param>
        /// <param name="decrement">当前缓存对象的减量<br/>
        /// key存在，则在该缓存对象上执行增量累加<br/>
        /// 不存在则将增量作为初始值缓存
        /// </param>
        /// <param name="cacheNumOfMinutes">缓存对象失效时间，小于0则没有永久存储<br/>单位：分钟</param>
        /// <param name="lockTimeOut">获取锁等待超时时间，超过该时间仍不能获取到锁，抛出异常<br/> 单位：毫秒</param>
        /// <returns>执行完累加后的缓存对象</returns>
        /// <exception cref="System.ArgumentNullException">在参数key或者increment值为null时抛出</exception>
        /// <exception cref="System.ArgumentException">在参数lockTimeOut小于0时抛出</exception>
        /// <exception cref="System.TimeoutException">在lockTimeOut时间之后仍然无法获取到couchbase锁时抛出</exception>
        public T DecrementWithLock<T>(string key, T decrement, int cacheNumOfMinutes, int lockTimeOut, string bucketName) where T : class, IDecrementer
        {
            if (key == null)
                throw new ArgumentNullException("key");
            if (decrement == null)
                throw new ArgumentNullException("decrement");
            if (lockTimeOut < 0)
                throw new ArgumentException("timeOut less than zero");

            T old = default(T);
            Action<IOperationResult<object>> callback = lockResult =>
            {
                if (lockResult.Value == null)
                {
                    old = default(T);
                    return;
                }

                var obj = lockResult.Value;
                old = Deserialize<T>(obj);
                old.Subtract(decrement);
                UpdateCas(key, old, cacheNumOfMinutes, lockResult.Cas, bucketName);
            };

            GetWithLock(key, lockTimeOut, callback, bucketName);

            return old;
        }

        /// <summary>
        /// 获取指定键的获取已加锁的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="getLockTimeOut">尝试获取锁超时时间：毫秒</param>
        /// <param name="lockTimeOut">加锁时间：毫秒</param>
        /// <param name="bucketName">bucket名称</param>
        /// <returns>
        /// 持有锁的结果<br/>
        /// key 不存在，返回空对象
        /// key 存在，获取到锁返回持有锁的对象，未能获取到锁（被其他线程获取未释放）返回空对象
        /// </returns>
        [Obsolete("已废弃，请勿调用")]
        public IOperationResult<object> GetWithLock(string key, int getLockTimeOut, int lockTimeOut, string bucketName)
        {
            IOperationResult<object> result;
            var now = DateTime.Now;
            var bucket = this.GetBucket(bucketName);
            if (!bucket.Exists(key))
            {
                bucket.Upsert(key, new object(), TimeSpan.FromMilliseconds(lockTimeOut + 60 * 1000));
            }
            while (true)
            {
                result = bucket.GetAndLock<object>(key, TimeSpan.FromMilliseconds(lockTimeOut));
                if (result.Cas > 0 || result.Value != null)
                {
                    break;
                }

                if ((DateTime.Now - now).TotalMilliseconds > getLockTimeOut)
                    throw new TimeoutException(string.Format("Get the lock timeout({0} ms)", getLockTimeOut));

                Thread.Sleep(5);
            }

            return result;
        }

        /// <summary>
        /// Gets the and lock.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="lockTimeOut">The lock time out.</param>
        /// <param name="bucketName">Name of the bucket.</param>
        /// <returns>Tuple&lt;System.UInt64, T&gt;.</returns>
        public Tuple<ulong, T> GetAndLock<T>(string key, int lockTimeOut, string bucketName)
        {
            var bucket = this.GetBucket(bucketName);
            var result = bucket.GetAndLock<byte[]>(key, TimeSpan.FromMilliseconds(lockTimeOut));
            return new Tuple<ulong, T>(result.Cas, Deserialize<T>(result.Value));
        }

        #region 私有方法

        /// <summary>
        /// 获取指定键的获取已加锁的值
        /// <para>调用该方法将导致当前线程阻塞，直到获取到加锁的缓存数据执行完回调方法或超时时间到</para>
        /// </summary>
        /// <param name="key">couchbase缓存key</param>
        /// <param name="lockTimeOut">加锁时间，单位：ms</param>
        /// <param name="getWithLockSuccess">获取到加锁的缓存数据后回调的委托</param>
        private void GetWithLock(string key, int lockTimeOut, Action<IOperationResult<object>> getWithLockSuccess, string bucketName)
        {
            if (getWithLockSuccess == null)
            {
                return;
            }
            using (ManualResetEvent resetEvent = new ManualResetEvent(false))
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                Action<IOperationResult<object>> action = lockResult =>
                {
                    getWithLockSuccess(lockResult);
                    resetEvent.Set();
                };
                ThreadQueue threadQueue;
                _keyThreadQueue.TryGetValue(key, out threadQueue);
                var count = threadQueue == null ? 0 : threadQueue.WaitQueue.Count;

                AddGetWithLockRequest(key, lockTimeOut, lockTimeOut, action, cts.Token, bucketName);
                var wait = resetEvent.WaitOne(lockTimeOut);
                if (!wait)
                {
                    cts.Cancel();
                    throw new TimeoutException(string.Format("Get the lock timeout({0} ms), There are {1} waiting threads", lockTimeOut, count));
                }
                if (count == 0)
                {
                    _keyThreadQueue.TryRemove(key, out threadQueue);
                }
            }
        }

        /// <summary>
        /// 增加获取分布式锁请求
        /// 主要用于多个线程同时获取同一个couchbase缓存key的加锁数据，将并发转换成队列等待的回调方式处理
        /// </summary>
        /// <param name="key">couchbase缓存key</param>
        /// <param name="getLockTimeOut">获取锁的超时时间，单位：ms</param>
        /// <param name="lockTimeOut">持有锁的时间，单位：ms</param>
        /// <param name="action">获取到加锁的数据回调事件</param>
        /// <param name="cancelToken">等待线程取消通知，用于取消当前线程获取加锁数据操作</param>
        private void AddGetWithLockRequest(string key, int getLockTimeOut, int lockTimeOut, Action<IOperationResult<object>> action, CancellationToken cancelToken, string bucketName)
        {
            ThreadQueue threadQueue;
            if (!_keyThreadQueue.TryGetValue(key, out threadQueue))
            {
                lock (_keyThreadQueueLock)
                {
                    if (!_keyThreadQueue.TryGetValue(key, out threadQueue))
                    {
                        threadQueue = new ThreadQueue { HasProcessThread = false, WaitQueue = new ConcurrentQueue<CanCancelCallback>() };
                        var addResult = _keyThreadQueue.TryAdd(key, threadQueue);
                        Log.Write(string.Format("添加key:({0})的等待队列，状态：{1}", key, addResult), MessageType.Info, GetType());
                    }
                }
            }
            threadQueue.WaitQueue.Enqueue(new CanCancelCallback { Callback = action, CancelToken = cancelToken });
            if (!threadQueue.HasProcessThread)
            {
                lock (threadQueue)
                {
                    if (!threadQueue.HasProcessThread)
                    {
                        CreateThreadForCacheKey(key, getLockTimeOut, lockTimeOut, threadQueue, bucketName);
                    }
                }
            }
        }

        /// <summary>
        /// 创建一个线程用于处理获取当前Couchbase缓存key的分布式缓存锁的线程队列
        /// <para>多个线程需要获取同一个Couchbase缓存key的分布式锁，将这些线程排队，使用一个新线程对这些排队的线程进程处理</para>
        /// </summary>
        /// <param name="key">couchbase缓存key</param>
        /// <param name="getLockTimeOut">获取锁的超时时间，单位：ms</param>
        /// <param name="lockTimeOut">持有锁的时间，单位：ms</param>
        /// <param name="threadQueue">等待获取当前Couchbase缓存key的分布式缓存锁的线程队列</param>
        private void CreateThreadForCacheKey(string key, int getLockTimeOut, int lockTimeOut, ThreadQueue threadQueue, string bucketName)
        {
            Action action = () =>
            {
                threadQueue.HasProcessThread = true;
                try
                {
                    while (!threadQueue.WaitQueue.IsEmpty)
                    {
                        CanCancelCallback queueItem;
                        if (!threadQueue.WaitQueue.TryDequeue(out queueItem) || queueItem == null || queueItem.Callback == null || queueItem.CancelToken.IsCancellationRequested)
                        {
                            Log.Write(string.Format("获取分布式锁的线程被跳过,key：{0}，queueItem 为null:{1}，queueItem.Callback 为null：{2}， queueItem.CancelToken.IsCancellationRequested：{3}",
                                                            key, queueItem == null, queueItem == null || queueItem.Callback == null, queueItem == null || queueItem.CancelToken.IsCancellationRequested)
                                , MessageType.Warn, GetType());
                            continue;
                        }
                        Log.Write(string.Format("处理获取分布式锁，key:{0}", key), MessageType.Debug, GetType());
                        ProcessGetWithLock(key, getLockTimeOut, lockTimeOut, queueItem, bucketName);
                    }
                }
                finally
                {
                    threadQueue.HasProcessThread = false;
                }
            };
            _getCASTaskPool.AddTask(states =>
            {
                action();
            }, null);
        }

        /// <summary>
        /// 处理同一个couchbase缓存key获取带锁的缓存数据
        /// 多个线程同时到达获取加锁的缓存数据时，采用队列回调的方式，一旦获取到加锁的缓存数据之后执行回调
        /// <para>
        /// 当前机器拿到加锁的数据之后，睡眠一会（1ms），以便其他机器程序能拿到当前锁
        /// 当前机器拿不到锁，睡眠时间较短（0ms，以便cpu处理其他线程），以便增大拿到当前加锁数据的几率
        /// </para>
        /// </summary>
        /// <param name="key">couchbase缓存key</param>
        /// <param name="getLockTimeOut">获取锁的超时时间，单位：ms</param>
        /// <param name="lockTimeOut">持有锁的时间，单位：ms</param>
        /// <param name="canCancelCallback">可以取消的回调对象</param>
        private void ProcessGetWithLock(string key, int getLockTimeOut, int lockTimeOut, CanCancelCallback canCancelCallback, string bucketName)
        {
            IOperationResult<byte[]> result;
            var now = DateTime.Now;
            var bucket = GetBucket(bucketName);
            if (!bucket.Exists(key))
            {
                bucket.Upsert(key, null as object, TimeSpan.FromMilliseconds(lockTimeOut + 60 * 1000));
            }
            while (!canCancelCallback.CancelToken.IsCancellationRequested)
            {
                result = bucket.GetAndLock<byte[]>(key, (uint)lockTimeOut / 1000);
                if (result.Cas > 0 || result.Value != null)
                {
                    Log.Write(string.Format("获取到加锁数据,key:{0}", key), MessageType.Debug);
                    DoWork(canCancelCallback.Callback, result, key, bucketName);
                    Thread.Sleep(2);
                    break;
                }
                if ((DateTime.Now - now).TotalMilliseconds > getLockTimeOut)
                {
                    break;
                }
                Thread.Sleep(0);
            }
        }

        /// <summary>
        /// 执行获取到分布式锁之后处理的回调事件<br />
        /// 如果执行过程有异常，吞掉所有异常，异常应该由委托传入方捕获处理
        /// </summary>
        /// <param name="work">回调的委托对象</param>
        /// <param name="input">分布式锁对象</param>
        /// <param name="key">couchbase缓存key</param>
        /// <param name="bucketName">bucket名称</param>
        private void DoWork(Action<IOperationResult<object>> work, IOperationResult<object> input, string key, string bucketName)
        {
            try
            {
                if (work == null)
                {
                    Log.Write(string.Format("获取到分布式锁，因为没有回调被跳过，Key：{0}", key), MessageType.Warn, GetType());
                    return;
                }
                work.Invoke(input);
            }
            catch
            {
            }
            finally
            {
                if (input.Cas > 0)
                {
                    var bucket = this.GetBucket(bucketName);
                    bucket.Unlock(key, input.Cas);
                }
            }
        }


        /// <summary>
        ///  获取object的byte
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private byte[] GetBytes(object o)
        {
            if (o == null)
                return null;

            byte[] bytes = null;
            using (var stream = new MemoryStream())
            {

                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, o);
                    ms.Position = 0;

                    using (GZipStream compressionStream = new GZipStream(stream, CompressionMode.Compress))
                    {
                        ms.CopyTo(compressionStream);

                    }
                }
                bytes = stream.ToArray();
            }

            return bytes;
        }

        /// <summary>
        /// 将bytes写入到MemoryStream中
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private Stream ToStream(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                ms.Flush();
                ms.Position = 0;
                MemoryStream stream = new MemoryStream();
                using (GZipStream decompressStream = new GZipStream(ms, CompressionMode.Decompress))
                {
                    decompressStream.Flush();
                    decompressStream.CopyTo(stream);
                    stream.Position = 0;
                    return stream;
                }

            }
        }

        /// <summary>
        /// 对象反序列化
        /// </summary>
        /// <typeparam name="T">反序列化后对象类型</typeparam>
        /// <param name="obj">待反序列化对象（流或byte数组）</param>
        /// <returns>反序列化后对象</returns>
        private T Deserialize<T>(object obj)
        {
            if (obj is Stream)
                return Serializer.Deserialize<T>(obj as Stream);

            var bytes = obj as byte[];
            if (bytes == null)
                return default(T);

            using (var stream = ToStream(bytes))
            {
                return Serializer.Deserialize<T>(stream);
            }
        }

        /// <summary>
        /// 更新指定缓存，有过期时间
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">键值</param>
        /// <param name="numOfMinutes">缓存时间，分钟计</param>
        /// <param name="cas">项目版本号，必须匹配才能更新成功</param>
        private void UpdateCas(string key, object value, long numOfMinutes, ulong cas, string bucketName)
        {
            IOperationResult<byte[]> result = null;
            var bucket = GetBucket(bucketName);
            if (numOfMinutes < 0)
            {
                result = bucket.Upsert(key, GetBytes(value), cas);
            }
            else
            {
                var timeSpan = new TimeSpan(0, (int)numOfMinutes, 0);
                result = bucket.Upsert(key, GetBytes(value), cas, timeSpan);
            }

            if (!result.Success)
            {
                if (result.Exception != null)
                    throw result.Exception;
                throw new Exception(String.Format("Couchbase Error Code: {0},Message: {1}", result.Status, result.Message));
            }
        }

        /// <summary>
        /// 开启couchbase对应的bucket
        /// 因为开启关闭bucket会有性能消耗，故将bucket进行静态缓存
        /// </summary>
        /// <param name="bucketName">bucket名称</param>
        private IBucket OpenBucket(string bucketName)
        {
            return _instance.OpenBucket(bucketName);
        }

        #endregion
    }
}
