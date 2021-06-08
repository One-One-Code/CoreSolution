using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleTest
{
    using CommonServiceLocator;
    using OneOne.Core.IOC.AutoFac;
    using OneOne.Core.Mq.Common;
    using OneOne.Core.Mq.Kafka;
    using OneOne.Core.Mq.RabbitMQ;
    using System.Threading;

    public class MqTest
    {
        private static int publishCount = 0;
        private static int consumerCount = 0;
        #region kafka test
        public static void KafkaTest()
        {
            var manager = new KafKaManager(null);
            manager.SubscribeAt<TestEvent>(new List<string> { "TestEvent1" }, "1", "172.20.6.139", new TestConsumer());
            //var manager1 = new KafKaManager("172.20.6.139", null);
            //while (true)
            //{

            //    var @event = new TestEvent() { Text = Guid.NewGuid().ToString() };
            //    manager1.Publish(@event);
            //    publishCount++;
            //}

        }

        public class TestEvent : EventBase
        {
            public override string TopicName => "TestEvent1";

            public string Text { get; set; }
        }

        public class TestConsumer : ConsumerBase<TestEvent>
        {
            protected override void Receive(TestEvent eEvent)
            {

                Console.WriteLine($"TestConsumer接收到消息，消息内容为：{eEvent.Text}");
                consumerCount++;
            }

        }
        #endregion

        #region RabbitMQ Test
        public static void RabbitMQTest()
        {
            var _container = new AutoFacServiceLocator();

            _container.ScanAssemblyAsPerLifetimeScope<TestConsumer1>();
            _container.Map<IExceptionProcess, ExceptionProcess>();
            RegisterModule.Register(_container);
            _container.UseAsDefault(true);
            var config = new RabbitMQConfig { Host = "localhost:/mis", Username = "admin", Password = "123456" };
            //_container.BusOnRabbitMq(config, x => x.SubscribeAt(config.Host, new DefaultConsumeConfigurator(_container)));
            _container.BusOnRabbitMq(config, x => x.PublishAt(config.Host, new DefaultConsumeConfigurator(_container)));
            var publisher = ServiceLocator.Current.GetInstance<IEventPublisher>();
            publisher.Publish<RabbitTestEvent1>(new RabbitTestEvent1 { UserId = 2, UserName = Guid.NewGuid().ToString() }, "mis");
            while (true)
            {
                publisher.Publish<RabbitTestEvent>(new RabbitTestEvent { UserId = 2, UserName = Guid.NewGuid().ToString() });
                publisher.Publish<RabbitTestEvent2>(new RabbitTestEvent2 { UserId =3, UserName = Guid.NewGuid().ToString() });
            }
        }

    }


    public class RabbitTestEvent : Event
    {
        public string UserName { get; set; }

        public int UserId { get; set; }

        public override string RoutingKey => string.Empty;
        public override string ExchangeName => string.Empty;
        public override string ExchangeType => "fanout";
        public override string QueueName => "TestQueue";

        public override bool ExceptionRequeue => true;
    }

    public class RabbitTestEvent1 : Event
    {
        public string UserName { get; set; }

        public int UserId { get; set; }

        public override string RoutingKey => "prex-TestQueue1";
        public override string ExchangeName => "directExchange";
        public override string ExchangeType => "direct";
        public override string QueueName => "TestQueue1";
    }

    public class RabbitTestEvent2 : Event
    {
        public string UserName { get; set; }

        public int UserId { get; set; }
        public override string RoutingKey => "prex-TestQueue2";
        public override string ExchangeName => "directExchange";
        public override string ExchangeType => "direct";
        public override string QueueName => "TestQueue2";
    }

    public class TestConsumer0 : OneOne.Core.Mq.RabbitMQ.IEventConsumer<RabbitTestEvent>
    {
        public void Consume(RabbitTestEvent eEvent)
        {
            Console.WriteLine($"ManagedThreadId:{Thread.CurrentThread.ManagedThreadId}{eEvent.UserId}:{eEvent.UserName}");
            //Thread.Sleep(10 * 1000);
            throw new RequeueException();
        }
    }

    public class TestConsumer1 : OneOne.Core.Mq.RabbitMQ.IEventConsumer<RabbitTestEvent1>
    {
        public void Consume(RabbitTestEvent1 eEvent)
        {
            Console.WriteLine($"TestEvent1,ManagedThreadId:{Thread.CurrentThread.ManagedThreadId}{eEvent.UserId}:{eEvent.UserName}");
        }
    }

    public class TestConsumer2 : OneOne.Core.Mq.RabbitMQ.IEventConsumer<RabbitTestEvent2>
    {
        public void Consume(RabbitTestEvent2 eEvent)
        {
            Console.WriteLine($"ManagedThreadId:{Thread.CurrentThread.ManagedThreadId}{eEvent.UserId}:{eEvent.UserName}");
            Thread.Sleep(10 * 1000);
        }
    }

    public class ExceptionProcess : IExceptionProcess
    {

        public bool Process<T>(T message, Uri uri)
        {
            var messObj = message as Event;
            messObj.RetryCount = 0;
            var virtualHost = uri.AbsolutePath.Split('/')[1];
            return true;
        }
    }
    #endregion
}


