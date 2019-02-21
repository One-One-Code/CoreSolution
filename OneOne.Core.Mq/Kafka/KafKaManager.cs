using System;
using System.Collections.Generic;
using System.Text;

namespace OneOne.Core.Mq.Kafka
{
    using System.IO;
    using System.IO.Compression;

    using OneOne.Core.Logger;

    using ProtoBuf;

    using RdKafka;

    public class KafKaManager : IDisposable
    {
        private static Producer producer;
        private Dictionary<string, Topic> topics = new Dictionary<string, Topic>();

        public void Dispose()
        {
            if (this.topics.Count > 0)
            {
                foreach (var topic in this.topics)
                {
                    topic.Value.Dispose();
                }
                this.topics.Clear();
            }
            producer?.Dispose();
        }

        public KafKaManager(string server, IMessageSerialize serialize)
        {
            producer = new Producer($"{server}:9092");
            AdaptConfig.MessageSerializeInstance = serialize ?? new MessageJsonSerialize();
        }

        public KafKaManager(IMessageSerialize serialize)
        {
            AdaptConfig.MessageSerializeInstance = serialize ?? new MessageJsonSerialize();
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        public void Publish<T>(T message) where T : EventBase
        {
            var topic = this.GetTopic(message.TopicName);
            var messageBytes = AdaptConfig.MessageSerializeInstance.Serialize(message);
            var deliveryReport = topic.Produce(messageBytes).Result;
        }

        /// <summary>
        /// 启动消息监听
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="topicNames"></param>
        /// <param name="groupId">消费者组</param>
        /// <param name="host"></param>
        /// <param name="consumerAction">处理该类消息的逻辑类</param>
        public void SubscribeAt<T>(List<string> topicNames, string groupId, string host, IEventConsumer<T> consumerAction)
        {
            var config = new Config() { GroupId = groupId };
            var consumer = new EventConsumer(config, $"{host}:9092");
            consumer.OnMessage += (obj, msg) => { consumerAction.Consumer(msg); };
            consumer.OnConsumerError += (obj, error) => { Log.Write($"消费消息异常:{error.ToString()}", MessageType.Error); };
            consumer.Subscribe(topicNames);
            consumer.Start();
        }

        /// <summary>
        /// 根据topic名称的topic
        /// </summary>
        /// <param name="topicName"></param>
        /// <returns></returns>
        private Topic GetTopic(string topicName)
        {
            if (this.topics.ContainsKey(topicName))
            {
                return this.topics[topicName];
            }
            lock (this.topics)
            {
                if (this.topics.ContainsKey(topicName))
                {
                    return this.topics[topicName];
                }
                var topic = producer.Topic(topicName);
                this.topics[topicName] = topic;
                return topic;
            }
        }
    }
}
