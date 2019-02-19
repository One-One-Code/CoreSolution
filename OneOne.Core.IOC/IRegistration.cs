using System;
using System.Collections.Generic;
using System.Text;

namespace OneOne.Core.IOC
{
    using StructureMap.Building.Interception;
    using StructureMap.Configuration.DSL.Expressions;

    public interface IRegistration
    {
        /// <summary>
        /// 注册registry
        /// </summary>
        /// <param name="registry"></param>
        void Register(object registry);

        /// <summary>
        /// 扫描包含T类型的程序集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void ScanAssembly<T>();

        void ScanAssemblyAndConnectImplementationsToTypesClosing<T>(Type openGenericType);

        /// <summary>
        /// 将TInterface接口和TImplementation实现类绑定上
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="key"></param>
        /// <param name="singleton">true表示单例模式</param>
        void Map<TInterface, TImplementation>(string key = null, bool singleton = false) where TImplementation : TInterface;

        /// <summary>
        /// 使用function绑定TInterface的实现类
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="function"></param>
        /// <param name="key"></param>
        /// <param name="singleton">true表示单例模式</param>
        void Map<TInterface>(Func<TInterface> function, string key = null, bool singleton = false);

        /// <summary>
        /// 将instance绑定为TInterface接口的实现对象
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="instance"></param>
        void Inject<TInterface>(TInterface instance) where TInterface : class;

        /// <summary>
        /// 加载容器
        /// </summary>
        void Load();

        object CreateRegister();
    }
}
