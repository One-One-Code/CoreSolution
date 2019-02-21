using System;
using System.Collections.Generic;
using System.Text;

namespace OneOne.Core.Mq.Kafka
{
    using System.IO;
    using System.IO.Compression;

    using ProtoBuf;

    /// <summary>
    /// ProtoBuf对消息进行序列化操作
    /// </summary>
    public class MessageProtoBufSerialize : IMessageSerialize
    {
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public T Deserialize<T>(byte[] bytes)
        {
            if (bytes == null)
                return default(T);
            using (var ms = new MemoryStream(bytes))
            {
                ms.Position = 0;
                return Serializer.Deserialize<T>(ms);
            }
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public byte[] Serialize<T>(T obj)
        {
            if (obj == null)
                return null;

            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, obj);
                ms.Position = 0;
                return ms.ToArray();
            }
        }
    }
}
