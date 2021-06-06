using Autofac;
using Autofac.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OneOne.Core.IOC.AutoFac
{ /// <summary>
  /// AutoFac的注入操作接口
  /// </summary>
    public interface IAutoFacRegistration
    {
        /// <summary>
        /// 接口和实现类的单个匹配注册
        /// </summary>
        /// <typeparam name="TInterface">接口类型</typeparam>
        /// <typeparam name="TImplementation">实现类类型</typeparam>
        /// <param name="key">注册别名，一般在一个接口多个实现类的时候使用</param>
        /// <param name="singleInstance">是否为单例模式</param>
        void Map<TInterface, TImplementation>(string key = null, bool singleInstance = false);

        /// <summary>
        /// 接口和实现类的单个匹配注册（基于确切对象匹配）
        /// </summary>
        /// <typeparam name="TInterface">接口类型</typeparam>
        /// <typeparam name="TImplementation">实现类类型</typeparam>
        /// <param name="instance">实现类实例</param>
        /// <param name="key">注册别名，一般在一个接口多个实现类的时候使用</param>
        /// <param name="singleInstance">是否为单例模式</param>
        void Map<TInterface, TImplementation>(TImplementation instance, string key = null, bool singleInstance = false)
            where TImplementation : class;

        /// <summary>
        /// 扫描T类型所在程序集
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="singleInstance">是否为单例模式</param>
        void ScanAssembly<T>(bool singleInstance = false);

        /// <summary>
        /// 扫描程序集
        /// </summary>
        /// <param name="assemblies">程序集数组</param>
        /// <param name="singleInstance">是否为单例模式</param>
        void ScanAssembly(Assembly[] assemblies, bool singleInstance = false);

        /// <summary>
        /// 注册多个类型
        /// 类型一定是实现类类型，不能是接口类类型
        /// </summary>
        /// <param name="types">实现类的类型集合</param>
        /// <param name="singleInstance">是否为单例模式</param>
        void RegisterTypes(List<Type> types, bool singleInstance = false);

        /// <summary>
        /// 获取接口所有的实现类对象
        /// </summary>
        /// <typeparam name="TInterface">接口类型</typeparam>
        /// <returns>接口实现类的集合</returns>
        IEnumerable<TInterface> GetAllInstance<TInterface>();

        /// <summary>
        /// 获取接口的单个实现类对象
        /// </summary>
        /// <typeparam name="TInterface">接口类型</typeparam>
        /// <param name="key">注册时的别名</param>
        /// <returns>接口的实现对象</returns>
        TInterface GetInstance<TInterface>(string key = null);

        /// <summary>
        /// 注册对象和接口的关系
        /// </summary>
        /// <typeparam name="TInterface">接口类型</typeparam>
        /// <typeparam name="TImplementation">实现类类型</typeparam>
        /// <param name="instance">实现类对象</param>
        /// <param name="singleInstance">是否为单例模式</param>
        void InjectInstance<TInterface, TImplementation>(TImplementation instance, string key = null, bool singleInstance = false) where TImplementation : class;

        /// <summary>
        /// 开启一个新的生命周期
        /// </summary>
        /// <returns>新的生命周期对象</returns>
        ILifetimeScope BeginLifetimeScope();

        /// <summary>
        /// 注册模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void RegisterModule<T>() where T : IModule, new();
    }
}
