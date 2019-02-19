using System;
using System.Collections.Generic;
using System.Text;

namespace OneOne.Core.IOC.AutoFac
{
    public interface IAutoFacRegistration
    {
        void Map<TInterface, TImplementation>(string key = null);

        void Map<TInterface, TImplementation>(TImplementation instance, string key = null)
            where TImplementation : class;

        void ScanAssembly<T>();

        IEnumerable<TInterface> GetAllInstance<TInterface>();

        TInterface GetInstance<TInterface>(string key = null);
    }
}
