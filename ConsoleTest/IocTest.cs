using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleTest
{
    using CommonServiceLocator;

    using OneOne.Core.IOC;
    using OneOne.Core.IOC.AutoFac;

    public class IocTest
    {
        public static void Test()
        {
            var locator = new StructureMapServiceLocator(null,new LogInterceptor());
            locator.Map<IRegistration>(() => locator);
            //locator.Map(() => ServiceLocator.Current);
            locator.UseAsDefault();
            //locator.Map<IA, A>();
            //locator.Map<IB, B>("B");
            //locator.Map<IC, C>();
            //locator.Map<IB, D>("D");
            locator.ScanAssembly<IocTest>();
            locator.Map<E, E>();
            locator.Load();
            locator.GetInstance<IA>().Write();
            ServiceLocator.Current.GetInstance<IA>().Sing();
            var instances = ServiceLocator.Current.GetAllInstances<IB>();
            foreach (var instance in instances)
            {
                instance.Write();
            }
        }

        public static void AutoFacTest()
        {
            //如果需要进行AOP拦截的话则需要按照Autofac.Extras.DynamicProxy类库，实现IInterceptor接口
            var locator = new AutoFacServiceLocator();
            locator.ScanAssembly<IocTest>();
            //locator.Map<IB, B>("B");
            //locator.Map<IB, D>("D");
            //locator.Map<IB, D>();
            locator.UseAsDefault(true);
            //locator.GetInstance<IB>("B").Write();
            var instances = locator.GetAllInstance<IB>();
            var A=ServiceLocator.Current.GetInstance<IB>();
            A.Write();
            foreach (var instance in instances)
            {
                instance.Write();
            }
        }
    }

    public interface IA
    {
        void Write();

        void Sing();
    }

    public interface IB
    {
        void Write();
        void Sing();
    }

    public interface IC
    {
        void Write();
        void Sing();
    }

    public class A : IA
    {
        public void Write()
        {
            Console.WriteLine("I am A Write");
        }

        public void Sing()
        {
            Console.WriteLine("I am A Sing");
        }
    }

    public class B : IB
    {
        public void Write()
        {
            Console.WriteLine("I am B Write");
        }

        public void Sing()
        {
            Console.WriteLine("I am B Sing");
        }
    }

    public class C : IC
    {
        private IA _a;
        public C(IA a)
        {
            this._a = a;
        }
        public void Write()
        {
            if (this._a != null)
            {
                this._a.Write();
            }
            Console.WriteLine("I am C Write");
        }

        public void Sing()
        {
            Console.WriteLine("I am C Sing");
        }
    }

    public class D : IB
    {
        public void Write()
        {
            Console.WriteLine("I am D Write");
        }

        public void Sing()
        {
            Console.WriteLine("I am D Sing");
        }
    }

    public class E
    {
        public void Write()
        {
            Console.WriteLine("I am D Write");
        }

        public void Sing()
        {
            Console.WriteLine("I am D Sing");
        }
    }
}
