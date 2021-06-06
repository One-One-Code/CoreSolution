using GreenPipes;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneOne.Core.Mq.RabbitMQ.PipeFilter;

namespace OneOne.Core.Mq.RabbitMQ
{
    /// <summary>
    /// 中间件扩展方法
    /// </summary>
    public static class MiddlewareConfiguratorExtensions
    {
        /// <summary>
        /// 异常之后的逻辑处理
        /// 重试N次之后还异常则需要进行的逻辑处理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configurator"></param>
        public static void UseExceptionLogger(this IConsumePipeConfigurator configurator)
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var observer = new MessageFilterConfigurationObserver(configurator);
        }
    }
}
