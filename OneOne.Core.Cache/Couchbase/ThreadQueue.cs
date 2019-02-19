using System;
using System.Collections.Generic;
using System.Text;

namespace OneOne.Core.Cache.Couchbase
{
    using System.Collections.Concurrent;
    using System.Threading;

    using global::Couchbase;

    public class ThreadQueue
    {
        /// <summary>
        /// 等待回调的委托队列
        /// </summary>
        public ConcurrentQueue<CanCancelCallback> WaitQueue { get; set; }

        /// <summary>
        /// 是否有当前队列的处理线程
        /// </summary>
        public bool HasProcessThread { get; set; }
    }

    /// <summary>
    /// 分布式锁获取回调相关封装
    /// </summary>
    public class CanCancelCallback

    {
        /// <summary>
        /// 获取到锁之后回调委托
        /// </summary>
        public Action<IOperationResult<object>> Callback { get; set; }

        /// <summary>
        /// 用于取消获取分布式锁的通知对象
        /// </summary>
        public CancellationToken CancelToken { get; set; }
    }
}
