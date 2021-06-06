using MassTransit;
using MassTransit.RabbitMqTransport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneOne.Core.Mq.Common;

namespace OneOne.Core.Mq.RabbitMQ
{
    public interface IConsumeConfigurator
    {
        void Configure(IRabbitMqReceiveEndpointConfigurator cfg, IRabbitMqBusFactoryConfigurator fcg, List<IConsumer> consumers);

        void ConfigurePublisher(IRabbitMqBusFactoryConfigurator fcg);
    }
}
