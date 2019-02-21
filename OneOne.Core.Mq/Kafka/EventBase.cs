using System;
using System.Collections.Generic;
using System.Text;

namespace OneOne.Core.Mq.Kafka
{
    /// <summary>
    /// kafka消息的基类
    /// </summary>
    public abstract class EventBase
    {
        /// <summary>
        /// 消息的topic名称
        /// </summary>
        public abstract string TopicName { get; }
    }
}
