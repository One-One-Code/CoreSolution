using System;

namespace OneOne.Core.IOC
{
    using System.Collections.Generic;
    using System.Linq;

    using CommonServiceLocator;

    using StructureMap;
    using StructureMap.Building.Interception;
    using StructureMap.Configuration.DSL.Expressions;
    using StructureMap.DynamicInterception;
    using StructureMap.Graph;
    using StructureMap.Graph.Scanning;
    using StructureMap.Pipeline;
    using StructureMap.TypeRules;

    public class StructureMapServiceLocator : ServiceLocatorImplBase, IRegistration
    {
        private IContainer _container;
        private readonly List<Registry> _registries = new List<Registry>();

        public StructureMapServiceLocator(IContainer container, params ISyncInterceptionBehavior[] interceptor)
        {
            if (container != null)
            {
                _container = container;
                return;
            }

            if (interceptor == null || interceptor.Length == 0)
            {
                _container = new Container();
                return;
            }
            _container = new Container(x => x.Policies.Interceptors(new DynamicProxyInterceptorPolicy(interceptor)));
            return;

        }

        public StructureMapServiceLocator() : this(null, null)
        {
        }

        /// <summary>
        /// Registers the current StructureMapServiceLocator with Enterprise Library as the current locator.
        /// </summary>
        public void UseAsDefault()
        {
            Map<IRegistration>(() => this, null, true);
            ServiceLocator.SetLocatorProvider(() => this);

        }

        /// <summary>
        /// 外部添加注册器
        /// </summary>
        /// <param name="registry"></param>
        public void Register(object registry)
        {
            this._registries.Add(registry as Registry);
        }

        private Registry GetRegister()
        {
            if (this._registries.Count == 0)
            {
                this._registries.Add(new Registry());
            }
            return this._registries.First();
        }

        /// <summary>
        /// 扫描程序集
        /// </summary>
        /// <typeparam name="T">扫描包含该类型的程序集</typeparam>
        public void ScanAssembly<T>()
        {
            var registry = new Registry();
            registry.Scan(with =>
            {
                with.AssemblyContainingType<T>();
                with.WithDefaultConventions();
                with.RegisterConcreteTypesAgainstTheFirstInterface();

            });

            this.Register(registry);
        }

        public void ScanAssemblyAndConnectImplementationsToTypesClosing<T>(Type openGenericType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 接口和实现类匹配
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="key"></param>
        public void Map<TInterface, TImplementation>(string key = null, bool singleton = false)
            where TImplementation : TInterface
        {
            var registry = this.GetRegister();
            var forInterface = registry.For<TInterface>();
            var mapping = forInterface.Use<TImplementation>();
            if (!string.IsNullOrEmpty(key))
            {
                mapping.Named(key);
            }
            if (singleton)
            {
                forInterface.Singleton();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="function"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public void Map<TInterface>(Func<TInterface> function, string key = null, bool singleton = false)
        {
            var registry = this.GetRegister();
            var forInterface = registry.For<TInterface>();
            var mapping = forInterface.Use(() => function.Invoke());
            if (!string.IsNullOrEmpty(key))
            {
                mapping.Named(key);
            }

            if (singleton)
            {
                forInterface.Singleton();
            }
        }

        /// <summary>
        /// 将对象注册为接口实例
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="instance"></param>
        public void Inject<TInterface>(TInterface instance) where TInterface : class
        {
            this._container.Inject(instance);
        }

        public void Load()
        {
            foreach (var registry in this._registries)
            {
                _container.Configure(p => p.AddRegistry(registry));
            }
            this._registries.Clear();
        }

        public object CreateRegister()
        {
            var register = new Registry();
            this._registries.Add(register);
            return register;
        }

        #region ServiceLocator.Current的实现

        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return _container.GetInstance(serviceType);
            }
            else
            {
                return _container.GetInstance(serviceType, key);
            }
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            foreach (object obj in _container.GetAllInstances(serviceType))
            {
                yield return obj;
            }
        }

        #endregion
    }
}
