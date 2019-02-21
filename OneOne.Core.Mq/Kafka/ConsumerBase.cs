using System;
using System.Collections.Generic;
using System.Text;
using RdKafka;

namespace OneOne.Core.Mq.Kafka
{
    public abstract class ConsumerBase<T> : IEventConsumer<T> where T : EventBase
    {
        /// <summary>
        /// 消费消息方法
        /// 主要进行消息的预处理
        /// </summary>
        /// <param name="message">原始消息对象</param>
        public void Consumer(Message message)
        {
            var messageSerialize = AdaptConfig.MessageSerializeInstance ?? new MessageJsonSerialize();
            var @event = messageSerialize.Deserialize<T>(message.Payload);
            this.Receive(@event);
        }

        /// <summary>
        /// 消费者接收消息
        /// 被业务Consumer类进行实现
        /// </summary>
        /// <param name="eEvent">消息体</param>
        protected abstract void Receive(T eEvent);
    }
}
