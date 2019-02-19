using System;
using System.Collections.Generic;

namespace OneOne.Utility4Core.ExtensionsMethod
{
    /// <summary>
    /// 集合扩展方法
    /// </summary>
    public static class EnumerableExtension
    {
        /// <summary>
        /// 遍历集合，执行传入表达式
        /// </summary>
        /// <typeparam name="T">集合类型</typeparam>
        /// <param name="source">当前操作的集合</param>
        /// <param name="expression">要执行表达式</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> expression)
        {
            foreach (T element in source)
                expression(element);            
        }

        /// <summary>
        /// 遍历集合，执行传入表达式(传入表达式的int类型参数代表当前循环次数)
        /// </summary>
        /// <typeparam name="T">集合类型</typeparam>
        /// <param name="source">当前操作的集合</param>
        /// <param name="expression">要执行表达式</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> expression)
        {
            int i = 0;
            foreach (T element in source)
                expression(element, i++);
        }  
    }
}
