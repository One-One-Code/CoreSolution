using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneOne.Core.Mq.RabbitMQ
{
    /// <summary>
    /// 消费者服务总线
    /// </summary>
    public class EventConsumerBus : IEventConsumerBus
    {
        private bool _disposed;
        public IBusControl Bus { get; private set; }

        public EventConsumerBus(IBusControl bus)
        {
            Bus = bus;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                
            }

            Bus = null;
            _disposed = true;
        }
    }
}
