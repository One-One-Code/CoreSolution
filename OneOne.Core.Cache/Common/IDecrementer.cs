using System;
using System.Collections.Generic;
using System.Text;

namespace OneOne.Core.Cache.Common
{
    /// <summary>
    /// 提供自身类型递减功能
    /// </summary>
    public interface IDecrementer
    {
        /// <summary>
        /// 递减，在当前对象上执行减量操作
        /// </summary>
        /// <param name="decrement">减量</param>
        void Subtract(IDecrementer decrement);
    }
}
