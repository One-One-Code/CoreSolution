using CommonServiceLocator;
using GreenPipes;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Context;
using MassTransit.PipeConfigurators;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneOne.Core.Mq.Common;

namespace OneOne.Core.Mq.RabbitMQ.PipeFilter
{
    /// <summary>
    /// 异常后的处理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExceptionLoggerSpecification<T> : IPipeSpecification<ConsumeContext<T>>
     where T : class
    {
        public IEnumerable<ValidationResult> Validate()
        {
            return Enumerable.Empty<ValidationResult>();
        }

        public void Apply(IPipeBuilder<ConsumeContext<T>> builder)
        {
            builder.AddFilter(new LogMessageTypeFilter<T>());
        }
    }

    public class MessageFilterConfigurationObserver : ConfigurationObserver, IMessageConfigurationObserver
    {
        public MessageFilterConfigurationObserver(IConsumePipeConfigurator receiveEndpointConfigurator)
            : base(receiveEndpointConfigurator)
        {
            Connect(this);
        }

        public void MessageConfigured<TMessage>(IConsumePipeConfigurator configurator)
            where TMessage : class
        {
            var specification = new ExceptionLoggerSpecification<TMessage>();

            configurator.AddPipeSpecification(specification);
        }
    }

    /// <summary>
    /// 异常过滤器
    /// </summary>
    /// <typeparam name="T"></typeparam>

    public class LogMessageTypeFilter<T> :
    IFilter<ConsumeContext<T>> where T : class
    {
        public void Probe(ProbeContext context)
        {
        }

        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            try
            {
                await next.Send(context);
            }
            catch (Exception ex)
            {
                if (context.Message is Event)
                {
                    var @ee = context.Message as Event;
                    if (@ee.RetryCount >= ConstField.RetryCount + 1)
                    {
                        var process = ServiceLocator.Current.GetInstance<IExceptionProcess>();
                        if (process != null)
                        {
                            process.Process(context.Message, context.DestinationAddress);
                        }
                    }

                }
                throw;
            }
        }
    }
}
