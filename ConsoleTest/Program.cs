using System;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //IocTest.AutoFacTest();
            //CouchbaseTest.Test();
            MqTest.KafkaTest();
            Console.Read();
        }
    }
}
