using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneOne.Core.Mq.RabbitMQ
{
    public interface IBusConfiguration
    {
        void SubscribeAt(string host, IConsumeConfigurator configurator, ushort threadCount = 0);
        void PublishAt(string host, IConsumeConfigurator configurator);
    }
}
