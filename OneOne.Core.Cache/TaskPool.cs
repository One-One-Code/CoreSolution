using System;
using System.Collections.Generic;
using System.Text;

namespace OneOne.Core.Cache
{
    using System.Threading;

    /// <summary>
    /// 提供可自行控制的线程池
    /// </summary>
    public class TaskPool : IDisposable
    {
        #region Private Field

        private readonly object _lock = new object();

        private int _max = 10; //最大线程数
        private int _min = 1;  //最小线程数
        private int _increment = 2; //当活动线程不足的时候新增线程的增量

        private readonly Dictionary<string, Task> _allPool; //所有的线程
        private readonly Queue<Task> _freeQueue;  //空闲线程队列
        private readonly Dictionary<string, Task> _working;   //正在工作的线程
        private readonly Queue<WaitItem> _waitQueue;  //等待执行工作队列

        private readonly ManualResetEvent _completedEvent = new ManualResetEvent(true);
        private readonly ManualResetEvent _freeEvent = new ManualResetEvent(true);

        ThreadPriority _threadPriority = ThreadPriority.Normal;

        #endregion

        #region Pblich Field & Property

        /// <summary>
        /// 获取是否存在等待的任务
        /// </summary>
        public bool HasWaitTask
        {
            get
            {
                return _waitQueue.Count > 0;
            }
        }

        /// <summary>
        /// 获取是否存在执行中的任务
        /// </summary>
        public bool HasWorkingTask
        {
            get
            {
                return _working.Count > 0;
            }
        }

        /// <summary>
        /// Gets the working task count.
        /// </summary>
        /// <value>The working task count.</value>
        public int WorkingTaskCount
        {
            get
            {
                return _working.Count;
            }
        }

        public int AllTaskCount
        {
            get
            {
                return _allPool.Count;
            }
        }


        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskPool"/> class.
        /// </summary>
        /// <param name="threadPriority">The thread priority.</param>
        /// <param name="minThreadCount">The minimum thread count.</param>
        /// <param name="maxThreadCount">The maximum thread count.</param>
        public TaskPool(ThreadPriority threadPriority = ThreadPriority.Normal, int minThreadCount = 1, int maxThreadCount = 10)
        {
            _threadPriority = threadPriority;
            _min = minThreadCount;
            _max = maxThreadCount;
            _allPool = new Dictionary<string, Task>();
            _working = new Dictionary<string, Task>();
            _freeQueue = new Queue<Task>();
            _waitQueue = new Queue<WaitItem>();
            Task task;
            //先创建最小线程数的线程
            for (int i = 0; i < _min; i++)
            {
                task = new Task(_threadPriority);
                //注册线程完成时触发的事件
                task.WorkComplete += WorkComplete;
                //加入到所有线程的字典中
                _allPool.Add(task.Key, task);
                //因为还没加入具体的工作委托就先放入空闲队列
                _freeQueue.Enqueue(task);
            }

        }

        #endregion

        #region Public Method

        /// <summary>
        /// 设置最大线程数
        /// </summary>
        /// <param name="value">线程数量</param>
        public void SetMaxThread(int value)
        {
            lock (_lock)
            {
                if (value < _min)
                    throw new ArgumentException("最大线程数不能小于最小线程数");
                _max = value;
            }
        }

        /// <summary>
        /// 设置最小线程数
        /// </summary>
        /// <param name="value">线程数量</param>
        public void SetMinThread(int value)
        {
            lock (_lock)
            {
                if (value <= 0)
                    throw new ArgumentException("最小线程数必须大于0");
                _min = value;
            }
        }

        /// <summary>
        /// 设置增量
        /// </summary>
        /// <param name="value">线程增量</param>
        public void SetIncrement(int value)
        {
            lock (_lock)
            {
                _increment = value;
            }
        }


        /// <summary>
        /// 添加工作委托的方法
        /// </summary>
        /// <param name="taskItem"></param>
        /// <param name="context"></param>
        public void AddTask(WaitCallback taskItem, object context)
        {
            lock (_lock)
            {
                int count = _allPool.Values.Count;
                if (count < _max)
                {
                    Task task;
                    if (_freeQueue.Count == 0)
                    {
                        //如果没有空闲队列了，就根据增量创建线程
                        for (int i = 0; i < _increment; i++)
                        {
                            //判断线程的总量不能超过max
                            if ((count + i) >= _max)
                                break;

                            task = new Task(_threadPriority);
                            task.WorkComplete += WorkComplete;
                            //加入线程字典
                            _allPool.Add(task.Key, task);
                            //加入空闲队列
                            _freeQueue.Enqueue(task);
                        }
                    }
                    task = _freeQueue.Dequeue();
                    _working.Add(task.Key, task);
                    _completedEvent.Reset();

                    task.TaskWorkItem = taskItem;
                    task.ContextData = context;
                    task.Active();
                }
                else
                {
                    //如果线程达到max就把任务加入等待队列
                    _waitQueue.Enqueue(new WaitItem { Context = context, Works = taskItem });
                    if (_waitQueue.Count >= _max)
                        _freeEvent.Reset();
                }
            }
        }

        /// <summary>
        /// 导致当前线程在指定的时间内等待操作完成
        /// </summary>
        /// <param name="millisecondsTimeout">等待时间</param>
        /// <returns>已完成 true，未完成 false</returns>
        public bool WaitCompleted(int millisecondsTimeout)
        {
            return _completedEvent.WaitOne(millisecondsTimeout);
        }

        /// <summary>
        /// 导致当前线程在指定的时间内等待任务池空闲，可接受新任务
        /// </summary>
        /// <param name="millisecondsTimeout">等待时间</param>
        /// <returns>可接受新任务 true，不可接受新任务 false</returns>
        public bool WaitFree(int millisecondsTimeout)
        {
            return _freeEvent.WaitOne(millisecondsTimeout);
        }

        #endregion

        #region IDisposable 成员

        /// <summary>
        /// 回收资源
        /// </summary>
        public void Dispose()
        {
            foreach (Task task in _allPool.Values)
            {
                using (task)
                {
                    task.Close();
                }
            }
            _allPool.Clear();
            _working.Clear();
            _waitQueue.Clear();
            _freeQueue.Clear();
        }

        #endregion

        #region Private Method

        /// <summary>
        /// 线程执行完毕后的触发事件
        /// </summary>
        /// <param name="task"></param>
        private void WorkComplete(Task task)
        {
            lock (_lock)
            {
                // 检查是否有等待执行的操作，如果有等待的优先执行等待的任务
                if (_waitQueue.Count > 0)
                {
                    WaitItem item = _waitQueue.Dequeue();
                    task.TaskWorkItem = item.Works;
                    task.ContextData = item.Context;
                    task.Active();
                }
                else
                {
                    _working.Remove(task.Key);
                    // 如果没有等待执行的操作就回收多余的工作线程
                    if (_freeQueue.Count >= _min)
                    {
                        _allPool.Remove(task.Key);
                        task.Close();
                    }
                    else
                    {
                        // 如果没超过就把线程从工作字典放入空闲队列
                        task.ContextData = null;
                        task.TaskWorkItem = null;
                        _freeQueue.Enqueue(task);
                    }
                }

                if (_working.Count == 0 && _waitQueue.Count == 0)
                    _completedEvent.Set();
                if (_waitQueue.Count < _max)
                    _freeEvent.Set();
            }
        }

        #endregion

        /// <summary>
        /// 存储等待队列的类
        /// </summary>
        internal class WaitItem
        {
            public WaitCallback Works { get; set; }
            public object Context { get; set; }
        }
    }

    /// <summary>
    /// 线程包装器类
    /// </summary>
    internal class Task : IDisposable
    {
        private readonly AutoResetEvent _locks; //线程锁
        private readonly Thread _thread;  //线程对象
        private bool _working;  //线程是否工作

        public object ContextData
        {
            get;
            set;
        }

        /// <summary>
        /// 线程完成一次操作的事件
        /// </summary>
        public event Action<Task> WorkComplete;

        /// <summary>
        /// 线程体委托
        /// </summary>
        public WaitCallback TaskWorkItem;

        /// <summary>
        /// 用于字典的Key
        /// </summary>
        public string Key
        {
            get;
            set;
        }

        /// <summary>
        /// 初始化包装器
        /// </summary>
        public Task(ThreadPriority threadPriority = ThreadPriority.Normal)
        {
            //设置线程一进入就阻塞
            _locks = new AutoResetEvent(false);
            Key = Guid.NewGuid().ToString();
            _thread = new Thread(Work);
            _thread.Priority = threadPriority;
            _thread.IsBackground = true;
            _working = true;
            ContextData = new object();
            _thread.Start();
        }

        /// <summary>
        /// 唤醒线程
        /// </summary>
        public void Active()
        {
            _locks.Set();
        }

        /// <summary>
        /// 设置执行委托和状态对象
        /// </summary>
        /// <param name="action"></param>
        /// <param name="context"></param>
        public void SetWorkItem(WaitCallback action, object context)
        {
            TaskWorkItem = action;
            ContextData = context;
        }

        /// <summary>
        /// 线程体包装方法
        /// </summary>
        private void Work()
        {
            while (_working)
            {
                // 阻塞线程
                _locks.WaitOne();
                try
                {
                    if (TaskWorkItem != null)
                        TaskWorkItem(ContextData);
                }
                catch
                { }
                try
                {
                    //完成一次执行，必须触发回调事件
                    if (WorkComplete != null)
                        WorkComplete(this);
                }
                catch
                { }
            }
        }

        /// <summary>
        /// 关闭线程
        /// </summary>
        public void Close()
        {
            _working = false;
            TaskWorkItem = null;
            _locks.Set();
        }

        /// <summary>
        /// 回收资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                _thread.Abort();
            }
            catch
            { }
        }
    }
}
