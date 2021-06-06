using GreenPipes;
using MassTransit;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneOne.Core.Mq.Common;
using OneOne.Core.IOC.AutoFac;

namespace OneOne.Core.Mq.RabbitMQ
{
    public static class SubscriptionAdapt
    {
        public static Dictionary<string, IBusControl> PublishBusControl = new Dictionary<string, IBusControl>();

        public static IBusConfiguration BusOnRabbitMq(this IAutoFacRegistration reg, RabbitMQConfig mqConfig, Action<IBusConfiguration> config)
        {
            var busConfig = new BusConfiguration(reg, mqConfig);
            config(busConfig);
            return busConfig;
        }
    }

    /// <summary>
    /// 服务注册
    /// </summary>
    public class BusConfiguration : IBusConfiguration
    {
        private readonly IAutoFacRegistration _reg;
        private readonly RabbitMQConfig rabbitMQConfig;

        public BusConfiguration(IAutoFacRegistration reg, RabbitMQConfig config)
        {
            _reg = reg;
            rabbitMQConfig = config;
        }

        /// <summary>
        /// 发送方注册
        /// </summary>
        /// <param name="host"></param>
        /// <param name="queue"></param>
        public void PublishAt(string host, IConsumeConfigurator configurator)
        {
            var url = string.Format("rabbitmq://{0}", host);
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri(url), h =>
                {
                    h.Username(rabbitMQConfig.Username);
                    h.Password(rabbitMQConfig.Password);
                });
                configurator.ConfigurePublisher(cfg);
            });
            busControl.Start();

            var virtualName = host.Split('/').Last();
            if (!SubscriptionAdapt.PublishBusControl.ContainsKey(virtualName))
            {
                SubscriptionAdapt.PublishBusControl.Add(virtualName, busControl);
            }
        }

        /// <summary>
        /// 消费方注册
        /// </summary>
        /// <param name="host"></param>
        /// <param name="configurator"></param>
        public void SubscribeAt(string host, IConsumeConfigurator configurator, ushort threadCount = 0)
        {
            var url = string.Format("rabbitmq://{0}", host);
            var allEvents = _reg.GetAllInstance<IEvent>().ToList();
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri(url), h =>
                {
                    h.Username(rabbitMQConfig.Username);
                    h.Password(rabbitMQConfig.Password);

                });
                //cfg.UseConcurrencyLimit(2);
                foreach (var eventType in allEvents)
                {
                    var @event = eventType as Event;
                    cfg.ReceiveEndpoint(@event.QueueName, x =>
                    {
                        if (!string.IsNullOrEmpty(@event.ExchangeName))
                        {
                            x.ConfigureConsumeTopology = false;
                            x.Bind(@event.ExchangeName, a =>
                            {
                                a.RoutingKey = @event.RoutingKey;
                                a.ExchangeType = @event.ExchangeType;
                                a.Durable = true;
                                
                            });

                        }
                        x.UseMessageRetry(r =>
                        {
                            //按照2,4,8,16进行4次重试，单位秒
                            r.Exponential(ConstField.RetryCount, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(16), TimeSpan.FromSeconds(2));
                            r.Handle<RequeueException>();
                        });
                        //异常记录
                        x.UseExceptionLogger();
                        //启动的线程数量,不设置的时候一般默认为cpu核数的两倍
                        //PrefetchCount:unack的数量
                        //UseConcurrencyLimit 线程的数量
                        if (threadCount > 0)
                        {
                            x.PrefetchCount = threadCount;
                            cfg.UseConcurrencyLimit(threadCount);
                        }
                        var consumers = GetQueueConsumers(eventType.GetType());
                        configurator.Configure(x, cfg, consumers);
                    });
                }
            });
            busControl.Start();
        }

        /// <summary>
        /// 根据Queue的名字查找相应的消费Class
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        private List<IConsumer> GetQueueConsumers(Type eventType)
        {
            var result = new List<IConsumer>();
            var consumers = _reg.GetAllInstance<IConsumer>().ToList();
            foreach (var consumer in consumers)
            {
                var types = consumer.GetType().GetInterfaces();
                if (types != null)
                {
                    foreach (var @type in types)
                    {
                        if (type.IsGenericType && type.GenericTypeArguments.Contains(eventType))
                        {
                            result.Add(consumer);
                        }
                    }

                }
            }
            return result;
        }
    }
}
