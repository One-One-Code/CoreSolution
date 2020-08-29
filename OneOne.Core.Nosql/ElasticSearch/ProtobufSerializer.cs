using Elasticsearch.Net;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneOne.Core.Nosql.ElasticSearch
{
    /// <summary>
    /// 使用Protobuf进行数据序列化
    /// 使用gzip进行压缩
    /// </summary>
    public class ProtobufSerializer : IElasticsearchSerializer
    {
        public object Deserialize(Type type, Stream stream)
        {
            //using (var ms = new MemoryStream())
            //{
            //    using (GZipStream decompressStream = new GZipStream(stream, CompressionMode.Decompress))
            //    {
            //        decompressStream.Flush();
            //        decompressStream.CopyTo(ms);
            //        ms.Position = 0;
            //        return Serializer.Deserialize(type, stream);
            //    }
            //}
            return Serializer.Deserialize(type, stream);
        }

        public T Deserialize<T>(Stream stream)
        {
            //using (var ms = new MemoryStream())
            //{
            //    using (GZipStream decompressStream = new GZipStream(stream, CompressionMode.Decompress))
            //    {
            //        decompressStream.Flush();
            //        decompressStream.CopyTo(ms);
            //        ms.Position = 0;
            //        return Serializer.Deserialize<T>(ms);
            //    }
            //}
            return Serializer.Deserialize<T>(stream);
        }

        public Task<object> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            var o = Deserialize(type, stream);
            return Task.FromResult(o);
        }

        public Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            var o = Deserialize<T>(stream);
            return Task.FromResult(o);
        }

        public void Serialize<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.Indented)
        {
            //using (var ms = new MemoryStream())
            //{
            //    Serializer.Serialize(ms, data);
            //    ms.Position = 0;

            //    using (GZipStream compressionStream = new GZipStream(stream, CompressionMode.Compress))
            //    {
            //        ms.CopyTo(compressionStream);

            //    }
            //}
            Serializer.Serialize(stream, data);
        }

        public Task SerializeAsync<T>(T data, Stream stream, SerializationFormatting formatting = SerializationFormatting.Indented, CancellationToken cancellationToken = default(CancellationToken))
        {
            Serialize<T>(data, stream, formatting);
            return Task.CompletedTask;
        }
    }
}
