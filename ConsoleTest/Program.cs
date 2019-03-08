using System;

namespace ConsoleTest
{
    using System.Collections.Generic;

    class Program
    {
        static void Main(string[] args)
        {
            //IocTest.AutoFacTest();
            //CouchbaseTest.Test();
            //MqTest.KafkaTest();
            PostgreTest.ExecuteFunction();
            //FileTest.FileMd5Test();
            Console.Read();
        }
    }
}
