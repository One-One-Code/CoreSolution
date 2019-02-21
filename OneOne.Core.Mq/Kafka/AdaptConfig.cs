using System;
using System.Collections.Generic;
using System.Text;

namespace OneOne.Core.Mq.Kafka
{
    /// <summary>
    /// kafka消费者和生产者启动后的运行配置信息
    /// </summary>
    public static class AdaptConfig
    {
        /// <summary>
        /// 消息序列化操作对象
        /// </summary>
        public static IMessageSerialize MessageSerializeInstance;
    }
}
