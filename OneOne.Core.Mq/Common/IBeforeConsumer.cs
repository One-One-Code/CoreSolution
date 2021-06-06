using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneOne.Core.Mq.Common
{
    /// <summary>
    /// Consumer接收消息之前执行的操作
    /// </summary>
    public interface IBeforeConsumer
    {
        /// <summary>
        /// 操作
        /// </summary>
        /// <param name="event">Consumer接收的数据对象</param>
        void Execute(Event @event);
    }
}
