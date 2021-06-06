using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneOne.Core.Mq.Common;

namespace OneOne.Core.Mq.RabbitMQ
{
    public interface IEventConsumer<in T> : IConsumer
        where T : Event
    {
        void Consume(T eEvent);
    }
}
