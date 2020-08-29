using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Polly;
using Polly.Timeout;

namespace ConsoleTest
{
    public class PollyTest
    {
        /// <summary>
        /// 超时降级
        /// </summary>
        public static void TestFallBack()
        {
            try
            {
                var policyTimeout = Policy.Timeout(3, Polly.Timeout.TimeoutStrategy.Pessimistic);
                var policyException = Policy.Handle<TimeoutRejectedException>()
                            .Fallback(() =>
                            {
                                Console.WriteLine("TimeoutException occured");
                            })
                            .Wrap(policyTimeout);
                /*
                 Polly支持两种超时策略：
                 TimeoutStrategy.Pessimistic： 悲观模式
                    当委托到达指定时间没有返回时，不继续等待委托完成，并抛超时TimeoutRejectedException异常。
                 TimeoutStrategy.Optimistic：乐观模式
                    这个模式依赖于 co-operative cancellation，只是触发CancellationTokenSource.Cancel函数，需要等待委托自行终止操作。
                 */

                //var mainPolicy = Policy.Wrap(policyTimeout, policyException);
                policyException.Execute(() =>
                {
                    Console.WriteLine("Job Start...");
                    TimeOut();
                    Console.WriteLine("Job End...");
                });
            }
            catch (TimeoutRejectedException e)
            {

            }

        }

        /// <summary>
        /// 熔断
        /// </summary>
        public static void TestCircuitBreaker()
        {
            var policy = Policy.Handle<ArgumentException>().Or<NullReferenceException>()
            .CircuitBreaker(5, TimeSpan.FromSeconds(10));
            while (true)
            {
                try
                {
                    policy.Execute(() =>
                    {
                        Console.WriteLine("Job Start...");
                        //var money = GetMoney();
                        //if (money > 800)
                        //{
                        //    throw new Exception();
                        //}
                        throw new ArgumentException();
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("There's one unhandled exception : " + ex.Message);
                }
                Thread.Sleep(500);
            }
           
        }

        public static int GetMoney()
        {
            var random = new Random();
            var money = random.Next(500, 1500);
            return money;
        }

        public static void TimeOut()
        {
            Thread.Sleep(5 * 1000);
        }
    }
}
