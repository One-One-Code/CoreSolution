using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleTest
{
    using StructureMap.DynamicInterception;

    public class LogInterceptor : ISyncInterceptionBehavior
    {
        public IMethodInvocationResult Intercept(ISyncMethodInvocation methodInvocation)
        {
            Console.WriteLine("before excute");
            return methodInvocation.InvokeNext();
        }
    }
}
