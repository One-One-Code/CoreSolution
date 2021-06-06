using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneOne.Core.Mq.Common
{
    public interface IEvent
    {
        /// <summary>
        /// 事件唯一码
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        DateTime CreateTime { get; }

        /// <summary>
        /// 发送事件的服务器名
        /// </summary>
        string Machine { get; }

        /// <summary>
        /// 通过那台
        /// </summary>
        string FromApplication { get; }
    }

    /// <summary>
    /// 发送RabbitMQ的事件的基类
    /// </summary>
    public class Event : IEvent
    {
        public static readonly string DefaultMachineName = Environment.MachineName;

        public Event()
        {
            Id = Guid.NewGuid();
            CreateTime = DateTime.Now;
            Machine = DefaultMachineName;
#if F48
            FromApplication = System.Configuration.ConfigurationManager.AppSettings["AppName"];
#else
#endif
        }

        public Guid Id { get; set; }


        public DateTime CreateTime { get; set; }


        public string Machine { get; set; }


        public string FromApplication { get; set; }

        public int RequestFromType { get; set; }

        /// <summary>
        /// RoutingKey
        /// fanout模式不用给该字段赋值
        /// </summary>
        public virtual string RoutingKey { get; }

        /// <summary>
        /// 交换机名称
        /// fanout模式不用给该字段赋值
        /// </summary>
        public virtual string ExchangeName { get; }

        /// <summary>
        /// 交换机类型
        /// fanout模式不用给该字段赋值
        /// </summary>
        public virtual string ExchangeType { get; }

        /// <summary>
        /// 对应Queue的名字
        /// </summary>
        public virtual string QueueName { get; }

        /// <summary>
        /// Consumer异常是否需要重入队列
        /// </summary>
        public virtual bool ExceptionRequeue { get { return false; } }

        /// <summary>
        /// 已经重试的次数
        /// </summary>
        public int RetryCount { get; set; }
    }
}
