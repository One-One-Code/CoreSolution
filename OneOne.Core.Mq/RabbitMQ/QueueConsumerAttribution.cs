using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneOne.Core.Mq.RabbitMQ
{
    [AttributeUsage(AttributeTargets.Class)]
    public class QueueConsumerAttribution : Attribute
    {
        public string QueueName { get { return _queueName; } }
        private string _queueName { get; set; }

        public QueueConsumerAttribution(string queueName)
        {
            _queueName = queueName;
        }

    }
}
