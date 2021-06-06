using MassTransit.ConsumeConfigurators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneOne.Core.Mq.Common
{
    /// <summary>
    /// RabbitMQ配置对象
    /// </summary>
    public class SubscribeHost
    {
        public string Host { get; set; }
        public string QueueName { get; set; }
        public int CunsumerNum { get; set; }

        #region 当不关心交换机和队列关系，只是一对一的时候以下属性不需要赋值,默认使用fanout
        public string ExchangeType { get; set; }
        public string ExchangeName { get; set; }
        public string RoutingKey { get; set; }
        #endregion
    }
}
