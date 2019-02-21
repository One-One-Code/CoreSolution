using System;
using System.Collections.Generic;
using System.Text;

namespace OneOne.Core.Mq.Kafka
{
    /// <summary>
    /// 消息的序列化和反序列化接口
    /// </summary>
    public interface IMessageSerialize
    {
        /// <summary>
        /// 将byte数组转为消息对象
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        T Deserialize<T>(byte[] bytes);

        /// <summary>
        /// 将消息对象转为byte数组
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        byte[] Serialize<T>(T obj);
    }
}
