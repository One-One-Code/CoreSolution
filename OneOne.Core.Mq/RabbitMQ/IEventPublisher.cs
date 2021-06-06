using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneOne.Core.Mq.Common;

namespace OneOne.Core.Mq.RabbitMQ
{
    /// <summary>
    /// 消息发布接口
    /// </summary>
    public interface IEventPublisher : IDisposable
    {
        /// <summary>
        /// 发布消息
        /// 同步发布
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="evt">消息对象</param>
        void Publish<T>(T evt) where T : Event;

        /// <summary>
        /// 发布消息
        /// 同步发布
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="evt"></param>
        /// <param name="virtualName">虚拟机名称</param>
        void Publish<T>(T evt, string virtualName) where T : Event;

        /// <summary>
        /// 往消息队列中发送消息
        /// 同步发布
        /// </summary>
        /// <param name="message">发送消息对象，必须是继承自Event的类型</param>
        /// <param name="messageType">发送消息Event类型对象</param>
        void Publish(object message, Type messageType);

        /// <summary>
        /// 往消息队列中发送消息
        /// 同步发布
        /// </summary>
        /// <param name="message">发送消息对象，必须是继承自Event的类型</param>
        /// <param name="messageType">发送消息Event类型对象</param>
        /// <param name="virtualName">虚拟机名称</param>
        void Publish(object message, Type messageType, string virtualName);
    }
}
