using System;
using System.Collections.Generic;
using System.Text;
using Castle.DynamicProxy;
using OneOne.Core.IOC.AutoFac;
using Polly;

namespace ConsoleTest
{
    /// <summary>
    /// 基于AutoFac的AOP
    /// 基于Polly的熔断机制
    /// </summary>
    public class AutoFacPollyTest
    {
        public static void Test()
        {
            var locator = new AutoFacServiceLocator();
            locator.Map<IAA, AA>(typeof(IInterceptor).Name);
            locator.Map<IInterceptor, InvokeManagerInterceptor>();
            locator.UseAsDefault(true);
            var result = locator.GetInstance<IAA>().GetName();
        }
    }

    public class AA : IAA
    {
        public string GetName()
        {
            return "huang";
        }
    }

    public interface IAA
    {
        string GetName();
    }

    public class InvokeManagerInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            var policy = Policy.Handle<TimeoutException>().Or<NullReferenceException>()
           .CircuitBreaker(5, TimeSpan.FromSeconds(10));
            try
            {
                policy.Execute(() =>
                {
                    Console.WriteLine($"Before Invoke {invocation.Method}");
                    invocation.Proceed();
                    Console.WriteLine($"After Invoke {invocation.Method}");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("There's one unhandled exception : " + ex.Message);
            }
        }
    }
}
