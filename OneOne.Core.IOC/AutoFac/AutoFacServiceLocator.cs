using System;
using System.Collections.Generic;
using System.Text;
using Autofac.Extras.CommonServiceLocator;

namespace OneOne.Core.IOC.AutoFac
{
    using Autofac;
    using Autofac.Core;
    using Autofac.Extras.DynamicProxy;
    using Castle.DynamicProxy;
    using CommonServiceLocator;
    using System.Reflection;

    /// <summary>
    /// 注册中心
    /// </summary>
    public class AutoFacServiceLocator : IAutoFacRegistration, IDisposable
    {
        private ContainerBuilder containerBuilder;

        private IContainer container;

        public AutoFacServiceLocator(ContainerBuilder containerBuilder)
        {
            this.containerBuilder = containerBuilder;

        }

        public AutoFacServiceLocator()
        {
            this.containerBuilder = new ContainerBuilder();

        }

        public void Dispose()
        {
            if (this.container != null)
                this.container.Dispose();
        }

        /// <summary>
        /// 开启一个生命周期
        /// </summary>
        /// <returns></returns>
        public ILifetimeScope BeginLifetimeScope()
        {
            return container.BeginLifetimeScope();
        }

        /// <summary>
        /// 默认加载
        /// </summary>
        /// <param name="build">true表示方法内会进行容器构建，否则不构建，由外部build，一般用于core 3+</param>
        public void UseAsDefault(bool build)
        {
            Map<IAutoFacRegistration, AutoFacServiceLocator>(this, null, true);

            containerBuilder.RegisterBuildCallback(c =>
            {
                var container = c as IContainer;
                var csl = new AutofacServiceLocator(container);

                ServiceLocator.SetLocatorProvider(() => csl);
            });
            if (build)
            {
                this.container = this.containerBuilder.Build();
            }
        }

        /// <summary>
        /// 注册对象和接口的关系
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="instance"></param>
        /// <param name="singleInstance"></param>
        public void InjectInstance<TInterface, TImplementation>(TImplementation instance, string key = null, bool singleInstance = false) where TImplementation : class
        {
            var reg = containerBuilder.RegisterInstance(instance).As<TInterface>().Named<TInterface>(key);
            if (singleInstance)
            {
                reg.SingleInstance();
            }
        }

        /// <summary>
        /// 建立接口关系
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="key"></param>
        /// <param name="singleInstance"></param>
        public void Map<TInterface, TImplementation>(string key = null, bool singleInstance = false)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var reg = this.containerBuilder.RegisterType<TImplementation>().Named<TInterface>(key);
                if (singleInstance)
                {
                    reg.SingleInstance();
                }
            }
            else
            {
                var reg = this.containerBuilder.RegisterType<TImplementation>().As<TInterface>();
                if (singleInstance)
                {
                    reg.SingleInstance();
                }
            }

        }

        /// <summary>
        /// 建立接口关系 
        /// 每个生命周期一个实例
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="key"></param>
        public void MapAsPerLifetimeScope<TInterface, TImplementation>(string key = null)
        {
            if (!string.IsNullOrEmpty(key))
            {
                var reg = this.containerBuilder.RegisterType<TImplementation>().Named<TInterface>(key).InstancePerLifetimeScope();
            }
            else
            {
                var reg = this.containerBuilder.RegisterType<TImplementation>().As<TInterface>().InstancePerLifetimeScope();
            }

        }

        public void Map<TInterface, TImplementation>(TImplementation instance, string key = null, bool singleInstance = false) where TImplementation : class
        {
            if (!string.IsNullOrEmpty(key))
            {
                var reg = this.containerBuilder.RegisterInstance(instance).Named<TInterface>(key);
                if (singleInstance)
                {
                    reg.SingleInstance();
                }
            }
            else
            {
                var reg = this.containerBuilder.RegisterInstance(instance).As<TInterface>();
                if (singleInstance)
                {
                    reg.SingleInstance();
                }
            }
        }

        /// <summary>
        /// 特定作用域下的单例
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="instance"></param>
        /// <param name="key"></param>
        public void MapAsPerLifetimeScope<TInterface, TImplementation>(TImplementation instance, string key = null) where TImplementation : class
        {
            if (!string.IsNullOrEmpty(key))
            {
                var reg = this.containerBuilder.RegisterInstance(instance).Named<TInterface>(key).InstancePerLifetimeScope();
            }
            else
            {
                var reg = this.containerBuilder.RegisterInstance(instance).As<TInterface>().InstancePerLifetimeScope();
            }
        }

        public void ScanAssembly<T>(bool singleInstance = false)
        {
            var assembly = typeof(T).Assembly;
            var reg = this.containerBuilder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();
            if (singleInstance)
            {
                reg.SingleInstance();
            }
        }

        public void ScanAssemblyPerRequest<T>()
        {
            var assembly = typeof(T).Assembly;
            var reg = this.containerBuilder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();
            reg.InstancePerRequest();
        }

        public void ScanAssemblyAsPerLifetimeScope<T>()
        {
            var assembly = typeof(T).Assembly;
            var reg = this.containerBuilder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();
            reg.InstancePerLifetimeScope();
        }

        public void ScanAssembly(Assembly[] assemblies, bool singleInstance = false)
        {
            var reg = this.containerBuilder.RegisterAssemblyTypes(assemblies).AsImplementedInterfaces();
            if (singleInstance)
            {
                reg.SingleInstance();
            }
        }

        public void ScanAssemblyAsPerLifetimeScope(Assembly[] assemblies)
        {
            var reg = this.containerBuilder.RegisterAssemblyTypes(assemblies).AsImplementedInterfaces();
            reg.InstancePerLifetimeScope();
        }

        public void RegisterTypes(List<Type> types, bool singleInstance = false)
        {
            var reg = this.containerBuilder.RegisterTypes(types.ToArray()).AsImplementedInterfaces();
            if (singleInstance)
            {
                reg.SingleInstance();
            }
        }

        public void RegisterTypesAsPerLifetimeScope(List<Type> types)
        {
            var reg = this.containerBuilder.RegisterTypes(types.ToArray()).AsImplementedInterfaces();
            reg.InstancePerLifetimeScope();
        }

        public IEnumerable<TInterface> GetAllInstance<TInterface>()
        {
            return this.container.Resolve<IEnumerable<TInterface>>();
        }

        public TInterface GetInstance<TInterface>(string key = null)
        {
            if (!string.IsNullOrEmpty(key))
            {
                return this.container.ResolveNamed<TInterface>(key);
            }
            return this.container.Resolve<TInterface>();
        }

        /// <summary>
        /// 注册模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RegisterModule<T>() where T : IModule, new()
        {
            this.containerBuilder.RegisterModule<T>();
        }

        /// <summary>
        /// 根据类型注册
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RegisterType<T>()
        {
            this.containerBuilder.RegisterType<T>();
        }

    }
}
