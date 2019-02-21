using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleTest
{
    using OneOne.Core.Mq.Kafka;

    public class MqTest
    {
        public static void KafkaTest()
        {
            var manager = new KafKaManager(null);
            manager.SubscribeAt<TestEvent>(new List<string> { "TestEvent" }, "1", "127.0.0.1", new TestConsumer());
            var manager1 = new KafKaManager("127.0.0.1", null);
            while (true)
            {
                
                var @event = new TestEvent() { Text = Console.ReadLine() };
                manager1.Publish(@event);
            }
            
        }
    }

    public class TestEvent : EventBase
    {
        public override string TopicName => "TestEvent";

        public string Text { get; set; }
    }

    public class TestConsumer : ConsumerBase<TestEvent>
    {
        protected override void Receive(TestEvent eEvent)
        {
            Console.WriteLine($"TestConsumer接收到消息，消息内容为：{eEvent.Text}");
        }
    }
}
