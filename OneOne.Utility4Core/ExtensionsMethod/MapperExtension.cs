using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneOne.Utility4Core.ExtensionsMethod
{
    public static class MapperExtension
    {
        private static IMapper mapper = null;

        /// <summary>
        /// 扫描程序集的Profile创建mapper
        /// </summary>
        /// <param name="assemblies">程序集集合</param>
        public static void CreateMapperByAssembly(string[] assemblies)
        {
            var configuration = new MapperConfiguration(cfg => cfg.AddMaps(assemblies));
            mapper = configuration.CreateMapper();
        }

        /// <summary>
        ///  类型映射
        /// </summary>
        /// <remarks>
        /// <![CDATA[
        ///     备注:把A对象转换成User类型的对象
        ///     用法：A.MapTo<User>();
        /// ]]>
        /// </remarks>
        public static T MapTo<T, TR>(this TR obj)
        {
            if (mapper == null)
            {
                throw new Exception("请先创建mapper，调用CreateMapperByAssembly方法");
            }
            if (obj == null) return default(T);

            return mapper.Map<T>(obj);
        }

        /// <summary>
        /// 集合映射
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TR"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<T> MapTo<T, TR>(this List<TR> obj)
        {
            if (mapper == null)
            {
                throw new Exception("请先创建mapper，调用CreateMapperByAssembly方法");
            }
            return mapper.Map<List<TR>, List<T>>(obj);
        }
    }
}
