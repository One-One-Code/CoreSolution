using System;

namespace ConsoleTest
{
    using System.Collections.Generic;

    class Program
    {
        static void Main(string[] args)
        {
            //IocTest.Test();
            //IocTest.AutoFacTest();
            //CouchbaseTest.Test();
            //MqTest.KafkaTest();
            //PostgreTest.ExecuteFunction();
            //FileTest.FileMd5Test();
            //EsTest.Test();
            //PollyTest.TestCircuitBreaker();
            //AutoFacPollyTest.Test();
            //DiagnosticTest.Publish();
            // MqTest.RabbitMQTest();
            MapperTest.Test();
            Console.Read();
        }
    }
}
