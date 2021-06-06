using Autofac;
using MassTransit;
using MassTransit.RabbitMqTransport;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OneOne.Core.Mq.Common;
using OneOne.Core.IOC.AutoFac;
using OneOne.Core.Logger;

namespace OneOne.Core.Mq.RabbitMQ
{
    /// <summary>
    /// Event类型事件的消费者的配置
    /// </summary>
    public class DefaultConsumeConfigurator : IConsumeConfigurator
    {
        /// <summary>
        /// ioc注册对象
        /// </summary>
        private readonly IAutoFacRegistration _reg;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="reg">ioc注册对象</param>
        /// <param name="locator">ioc定位器</param>
        public DefaultConsumeConfigurator(IAutoFacRegistration reg)
        {
            _reg = reg;
        }

        /// <summary>
        /// 配置consumer
        /// </summary>
        /// <param name="cfg">配置</param>
        /// <param name="consumers">consumer类型集合</param>
        public void Configure(IRabbitMqReceiveEndpointConfigurator cfg, IRabbitMqBusFactoryConfigurator fcg, List<IConsumer> consumers)
        {
            Log.Write(string.Format("消费类型{0}个", consumers.Count), MessageType.Info);
            foreach (var consumer in consumers)
            {
                var type = consumer.GetType();
                if (type.IsGenericType)
                {
                    Log.Write(string.Format("跳过{0}因为是泛型", type.FullName), MessageType.Warn);
                    continue;
                }

                var consumerTypes = type.GetInterfaces()
                    .Where(d => d.IsGenericType && d.GetGenericTypeDefinition() == typeof(IEventConsumer<>))
                    .Select(d => d.GetGenericArguments().Single())
                    .Distinct();


                foreach (var eventConsumerType in consumerTypes)
                {
                    try
                    {
                        Invoke(this, new Type[] { eventConsumerType, type }, "ConsumerTo", cfg, fcg, type);

                    }
                    catch (Exception ex)
                    {
                        Log.Write(ex.Message, MessageType.Error, this.GetType(), ex);
                    }
                }
            }
        }

        /// <summary>
        /// 添加consumer
        // 消费者停止消费消息后，将异常抛出，让消息回滚至原队列中
        // 等待下次处理
        /// </summary>
        /// <typeparam name="TEvent">consumer的接收事件参数</typeparam>
        /// <typeparam name="TConsumer">consumer类型</typeparam>
        /// <param name="cfg">配置对象</param>
        private void ConsumerTo<TEvent, TConsumer>(IRabbitMqReceiveEndpointConfigurator cfg, IRabbitMqBusFactoryConfigurator fcg, Type handlerType)
            where TConsumer : IEventConsumer<TEvent>
            where TEvent : Event, new()
        {
            SetPublishConfig<TEvent>(fcg);

            cfg.Handler<TEvent>(async evnt =>
           {
               try
               {
                   if (evnt.Message == null)
                   {
                       Log.Write(string.Format("消息{0}对象为null，不进入消费逻辑", evnt.MessageId), MessageType.Warn, this.GetType());
                       return;
                   }
                   using (var scope = _reg.BeginLifetimeScope())
                   {
                       try
                       {
                           var beforeConsumer = scope.Resolve<IBeforeConsumer>();
                           if (beforeConsumer != null)
                               beforeConsumer.Execute(evnt.Message);
                       }
                       catch
                       {

                       }
                       scope.Resolve<IEventConsumer<TEvent>>().Consume(evnt.Message);
                       try
                       {
                           var afterConsumer = scope.Resolve<IAfterConsumer>();


                           if (afterConsumer != null)
                               afterConsumer.Execute(evnt.Message);
                       }
                       catch
                       {

                       }
                   }

               }
               catch (StopedConsumeException)
               {
                   await evnt.Publish(evnt.Message);
               }
               catch (RequeueException ex)
               {
                   ++evnt.Message.RetryCount;
                   Log.Write(string.Format("执行{0}错误,RequeueException，messageId:{1}", typeof(TConsumer),evnt.Message.Id), MessageType.Error, this.GetType(), ex);
                   throw;
               }
               catch (Exception ex)
               {
                   Log.Write(string.Format("执行{0}错误,messageId:{1}", typeof(TConsumer), evnt.Message.Id), MessageType.Error, this.GetType(), ex);
                   if (evnt.Message.ExceptionRequeue)
                   {
                       ++evnt.Message.RetryCount;
                       throw new RequeueException();
                   }
                   throw;
               }
           });
        }

        /// <summary>
        /// 线程阻塞，等待指定时间重新投递到mq
        /// 不用masstransit的retry原因是再集群模式下重新投递的方案会更好
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="evt"></param>
        private async void Requeue<T>(ConsumeContext<T> evt) where T : Event
        {
            evt.Message.RetryCount++;
            Thread.Sleep(((int)Math.Pow(2, evt.Message.RetryCount - 1)) * 1000);
            await evt.Publish(evt.Message);
        }

        /// <summary>
        /// 消息配置
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="fcg"></param>
        private void SetPublishConfig<TEvent>(IRabbitMqBusFactoryConfigurator fcg) where TEvent : Event, new()
        {
            var @event = new TEvent();
            if (!string.IsNullOrEmpty(@event.RoutingKey))
            {
                fcg.Send<TEvent>(x =>
                {
                    x.UseRoutingKeyFormatter(context => context.Message.RoutingKey);

                });
            }

            if (!string.IsNullOrEmpty(@event.ExchangeName))
            {
                fcg.Message<TEvent>(x =>
                {
                    try
                    {
                        x.SetEntityName(@event.ExchangeName);
                    }
                    catch (Exception e)
                    {

                    }

                });

            }
            fcg.Publish<TEvent>(x =>
            {
                x.ExchangeType = string.IsNullOrEmpty(@event.ExchangeType) ? ExchangeType.Fanout : @event.ExchangeType;

            });
        }

        /// <summary>
        /// 配置发布者
        /// 根据IEvent消息去做配置
        /// </summary>
        /// <param name="fcg"></param>
        public void ConfigurePublisher(IRabbitMqBusFactoryConfigurator fcg)
        {
            var allEvents = _reg.GetAllInstance<IEvent>().ToList();
            foreach (var eventType in allEvents)
            {
                var type = eventType.GetType();
                Invoke(this, new Type[] { type }, "SetPublishConfig", fcg);
            }

        }

        /// <summary>
        /// 调用泛型方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="genericTypes"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        private void Invoke<T>(T target, Type[] genericTypes, string methodName, params object[] args)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            var genericMethod = method.GetGenericMethodDefinition().MakeGenericMethod(genericTypes);
            genericMethod.Invoke(target, args);
        }

    }
}
