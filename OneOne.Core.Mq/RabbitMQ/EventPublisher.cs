using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneOne.Core.Mq.Common;
using OneOne.Core.Logger;

namespace OneOne.Core.Mq.RabbitMQ
{
    public class EventPublisher : IEventPublisher
    {
        /// <summary>
        /// 发送消息后执行的事件
        /// <para>可通过该事件注入保存发送消息的功能</para>
        /// </summary>
        public static event Action<Event> EventPublishAfter;

        public EventPublisher()
        {
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="evt"></param>
        public void Publish<T>(T evt) where T : Event
        {
            var bus = GetPublishBusControl(null);
            Publish(evt, bus);
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="evt"></param>
        /// <param name="virtualName">虚拟机名称</param>
        public void Publish<T>(T evt, string virtualName) where T : Event
        {
            var bus = GetPublishBusControl(virtualName);
            Publish(evt, bus);
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="evt"></param>
        /// <param name="bus">消息发布对象/param>
        public void Publish<T>(T evt, IBusControl bus) where T : Event
        {
            bus.Publish(evt).Wait();
            if (EventPublishAfter != null)
            {
                try
                {
                    EventPublishAfter(evt);
                }
                catch (Exception ex)
                {
                    Log.Write("发布消息后执行事件出错", MessageType.Error, GetType(), ex);
                }
            }
        }

        /// <summary>
        /// 往消息队列中发送消息
        /// </summary>
        /// <param name="message">发送消息对象，必须是继承自Event的类型</param>
        /// <param name="messageType">发送消息Event类型对象</param>
        public void Publish(object message, Type messageType)
        {
            var bus = GetPublishBusControl(null);
            Publish(message, messageType, bus);
        }

        /// <summary>
        /// 往消息队列中发送消息
        /// </summary>
        /// <param name="message">发送消息对象，必须是继承自Event的类型</param>
        /// <param name="messageType">发送消息Event类型对象</param>
        /// <param name="virtualName">虚拟机名称</param>
        public void Publish(object message, Type messageType, string virtualName)
        {
            var bus = GetPublishBusControl(virtualName);
            Publish(message, messageType, bus);
        }

        /// <summary>
        /// 往消息队列中发送消息
        /// </summary>
        /// <param name="message">发送消息对象，必须是继承自Event的类型</param>
        /// <param name="messageType">发送消息Event类型对象</param>
        public void Publish(object message, Type messageType, IBusControl bus)
        {
            if (messageType == null)
            {
                return;
            }
            if (!(message is Event))
            {
                throw new Exception("消息必须继承Event类型");
            }

            bus.Publish(message, messageType).Wait();
            var @event = (Event)message;
            if (EventPublishAfter != null)
            {
                try
                {
                    EventPublishAfter(@event);
                }
                catch (Exception ex)
                {
                    Log.Write("发布消息后执行事件出错", MessageType.Error, GetType(), ex);
                }
            }
        }

        /// <summary>
        /// 获取消息发送的对象
        /// </summary>
        /// <param name="virualName"></param>
        /// <returns></returns>
        private IBusControl GetPublishBusControl(string virualName)
        {
            if (SubscriptionAdapt.PublishBusControl.Count == 0)
            {
                throw new Exception("未找到消息控制对象");
            }
            if (string.IsNullOrEmpty(virualName))
            {
                return SubscriptionAdapt.PublishBusControl.First().Value;
            }
            if (SubscriptionAdapt.PublishBusControl.ContainsKey(virualName))
            {
                return SubscriptionAdapt.PublishBusControl[virualName];
            }
            throw new Exception("未找到消息控制对象");
        }

        public void Dispose()
        {

        }
    }
}
