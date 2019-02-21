using System;
using System.Collections.Generic;
using System.Text;

namespace OneOne.Core.Mq.Kafka
{
    using RdKafka;

    public interface IEventConsumer<T>: IConsumer
    {
        void Consumer(Message message);
    }

    public interface IConsumer
    {

    }
}
