using System;
using System.Collections.Generic;
using System.Text;

namespace OneOne.Core.Mq.Kafka
{
    using Newtonsoft.Json;

    /// <summary>
    /// 使用json进行消息的序列化
    /// </summary>
    public class MessageJsonSerialize : IMessageSerialize
    {
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return default(T);
            }
            string jsonObj = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            return JsonConvert.DeserializeObject<T>(jsonObj);
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public byte[] Serialize<T>(T obj)
        {
            var jsonObj = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(jsonObj);
        }
    }
}
