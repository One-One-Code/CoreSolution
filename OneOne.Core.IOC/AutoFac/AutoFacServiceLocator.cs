﻿using System;
using System.Collections.Generic;
using System.Text;

namespace OneOne.Core.IOC.AutoFac
{
    using Autofac;

    using CommonServiceLocator;

    public class AutoFacServiceLocator : IAutoFacRegistration, IDisposable
    {
        private readonly ContainerBuilder containerBuilder;

        private IContainer container;

        public AutoFacServiceLocator()
        {
            this.containerBuilder = new ContainerBuilder();
        }

        public void Dispose()
        {
            if (this.container != null)
                this.container.Dispose();
        }

        public void Load()
        {
            this.container = this.containerBuilder.Build();
            Map<IAutoFacRegistration, AutoFacServiceLocator>(this);
        }

        public void Map<TInterface, TImplementation>(string key = null)
        {
            if (!string.IsNullOrEmpty(key))
            {
                this.containerBuilder.RegisterType<TImplementation>().Named<TInterface>(key);
                return;
            }
            this.containerBuilder.RegisterType<TImplementation>().As<TInterface>();
        }

        public void Map<TInterface, TImplementation>(TImplementation instance, string key = null) where TImplementation : class
        {
            if (!string.IsNullOrEmpty(key))
            {
                this.containerBuilder.RegisterInstance(instance).Named<TInterface>(key);
                return;
            }
            this.containerBuilder.RegisterInstance(instance).As<TInterface>();
        }

        public void ScanAssembly<T>()
        {
            var assembly = typeof(T).Assembly;
            this.containerBuilder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();
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
    }
}
