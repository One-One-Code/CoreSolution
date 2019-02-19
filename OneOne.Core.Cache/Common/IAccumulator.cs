using System;
using System.Collections.Generic;
using System.Text;

namespace OneOne.Core.Cache.Common
{
    /// <summary>
    /// 提供自身类型累加功能
    /// </summary>
    public interface IAccumulator
    {
        /// <summary>
        /// 累加
        /// </summary>
        /// <param name="increment">增量</param>
        void Add(IAccumulator increment);
    }

    /// <summary>
    /// 提供累加功能
    /// </summary>
    /// <typeparam name="TIncrement">累加对象类型</typeparam>
    public interface IAccumulator<in TIncrement>
    {
        /// <summary>
        /// 累加
        /// </summary>
        /// <param name="increment">增量</param>
        void Add(TIncrement increment);
    }
}
