using OneOne.Utility4Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace OneOne.Utility4Core.ExtensionsMethod
{
    /// <summary>
    /// 字符串扩展方法
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// TryParse委托缓存
        /// </summary>
        private static readonly Dictionary<Type, Delegate> TryParses = new Dictionary<Type, Delegate>();

        /// <summary>
        /// 检测字符串是否为null或空字符串
        /// </summary>
        /// <param name="s">操作的字符串</param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        /// <summary>
        /// 验证字符串对象是否为空对象或空字符串，如果是的话，则执行传入表达式
        /// </summary>
        /// <param name="s">操作的字符串</param>
        /// <param name="expression">无返回值的表达式</param>
        public static void IsNullOrEmptyThen(this string s, Action<string> expression)
        {
            if (string.IsNullOrEmpty(s)) expression(s);
        }

        /// <summary>
        ///  验证字符串对象是否为空对象或空字符串，如果是的话，则执行传入表达式，并将表达式结果返回
        /// </summary>
        /// <param name="s">操作的字符串</param>
        /// <param name="expression">有返回值的表达式</param>
        /// <returns></returns>
        public static string IsNullOrEmptyThen(this string s, Func<string, string> expression)
        {
            if (string.IsNullOrEmpty(s)) return expression(s);
            return s;
        }

        /// <summary>
        /// 将字符串格式化并返回
        /// </summary>
        /// <param name="s">操作的字符串</param>
        /// <param name="formarParameter">格式化参数</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatWith(this string s, params object[] formarParameter)
        {
            return string.Format(s, formarParameter);
        }

        /// <summary>
        /// 将字符串格式化并返回
        /// </summary>
        /// <param name="s">操作的字符串</param>
        /// <param name="formarParameter">格式化参数</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatWith(this string s, object formarParameter)
        {
            return string.Format(s, formarParameter);
        }

        /// <summary>
        /// 将字符串格式化并返回
        /// </summary>
        /// <param name="s">操作的字符串</param>
        /// <param name="formarParameter1">格式化参数1</param>
        /// <param name="formarParameter2">格式化参数2</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatWith(this string s, object formarParameter1, object formarParameter2)
        {
            return string.Format(s, formarParameter1, formarParameter2);
        }

        /// <summary>
        /// 将字符串格式化并返回
        /// </summary>
        /// <param name="s">操作的字符串</param>
        /// <param name="formarParameter1">格式化参数1</param>
        /// <param name="formarParameter2">格式化参数2</param>
        /// <param name="formarParameter3">格式化参数3</param>
        /// <returns>格式化后的字符串</returns>
        public static string FormatWith(this string s, object formarParameter1, object formarParameter2, object formarParameter3)
        {
            return string.Format(s, formarParameter1, formarParameter2, formarParameter3);
        }

        #region 验证指定字符串在指定字符串数组中的位置

        /// <summary>
        /// 验证指定字符串是否属于指定字符串数组中的一个元素
        /// </summary>
        /// <param name="searchStr">指定字符串</param>
        /// <param name="arrStr">指定字符串数组</param>
        /// <returns>字符串在指定字符串数组中的位置</returns>
        public static bool IsInArray(this string searchStr, string[] arrStr)
        {
            return IsInArray(searchStr, arrStr, true);
        }
        /// <summary>
        /// 验证指定字符串是否属于指定字符串数组中的一个元素
        /// </summary>
        /// <param name="searchStr">指定字符串</param>
        /// <param name="arrStr">指定字符串数组</param>
        /// <param name="caseInsensetive">是否不区分大小写, true为区分, false为不区分</param>
        /// <returns>字符串在指定字符串数组中如不存在则返回false</returns>
        public static bool IsInArray(this string searchStr, string[] arrStr, bool caseInsensetive)
        {
            return GetIndexInArray(searchStr, arrStr, caseInsensetive) >= 0;
        }
        /// <summary>
        /// 判断指定字符串是否属于指定字符串数组中的一个元素
        /// </summary>
        /// <param name="searchStr">字符串</param>
        /// <param name="arrStr">内部以逗号分割单词的字符串</param>
        /// <returns>判断结果</returns>
        public static bool IsInArray(this string searchStr, string arrStr)
        {
            return IsInArray(searchStr, Split(arrStr, ","), false);
        }
        /// <summary>
        /// 验证指定字符串是否属于指定字符串数组中的一个元素
        /// </summary>
        /// <param name="searchStr">指定字符串</param>
        /// <param name="arrStr">指定内部以特殊符号分割的字符串</param>
        /// <param name="splitStr">特殊分割符号</param>
        /// <returns>字符串在指定字符串数组中如不存在则返回false</returns>
        public static bool IsInArray(this string searchStr, string arrStr, string splitStr)
        {
            return IsInArray(searchStr, Split(arrStr, splitStr), true);
        }
        /// <summary>
        /// 验证指定字符串是否属于指定字符串数组中的一个元素
        /// </summary>
        /// <param name="searchStr">字符串</param>
        /// <param name="arrStr">指定内部以特殊符号分割的字符串, true为区分, false为不区分</param>
        /// <param name="splitStr">分割字符串</param>
        /// <param name="caseInsensetive">是否不区分大小写, true为区分, false为不区分</param>
        /// <returns>验证结果</returns>
        public static bool IsInArray(this string searchStr, string arrStr, string splitStr, bool caseInsensetive)
        {
            return IsInArray(searchStr, Split(arrStr, splitStr), caseInsensetive);
        }
        #endregion

        #region 验证指定字符串在指定字符串数组中的位置

        /// <summary>
        /// 验证指定字符串在指定字符串数组中的位置
        /// </summary>
        /// <param name="searchStr">指定字符串</param>
        /// <param name="arrStr">指定字符串数组</param>
        /// <returns>字符串在指定字符串数组中的位置, 如不存在则返回-1</returns>		
        public static int GetIndexInArray(this string searchStr, string[] arrStr)
        {
            return GetIndexInArray(searchStr, arrStr, true);
        }
        /// <summary>
        /// 验证指定字符串在指定字符串数组中的位置
        /// </summary>
        /// <param name="searchStr">指定字符串</param>
        /// <param name="arrStr">指定字符串数组</param>
        /// <param name="caseInsensetive">是否区分大小写, true为区分, false为不区分</param>
        /// <returns>字符串在指定字符串数组中的位置, 如不存在则返回-1</returns>
        public static int GetIndexInArray(this string searchStr, string[] arrStr, bool caseInsensetive)
        {
            int retValue = -1;
            if (!string.IsNullOrEmpty(searchStr) && arrStr.Length > 0)
            {
                for (int i = 0; i < arrStr.Length; i++)
                {
                    if (caseInsensetive)
                    {
                        if (searchStr == arrStr[i])
                            retValue = i;
                    }
                    else
                    {
                        if (searchStr.ToLower() == arrStr[i].ToLower())
                            retValue = i;
                    }
                }
            }
            return retValue;
        }

        #endregion

        #region 分割字符串数组
        /// <summary>
        /// 分割字符串数组
        /// </summary>
        /// <param name="sourceStr">要分割字符串</param>
        /// <param name="splitStr">分割字符</param>
        /// <returns>分割后的字符串</returns>
        public static string[] Split(this string sourceStr, string splitStr)
        {
            if (!string.IsNullOrEmpty(sourceStr))
            {
                if (sourceStr.IndexOf(splitStr) < 0)
                {
                    string[] tmp = { sourceStr };
                    return tmp;
                }
                return Regex.Split(sourceStr, Regex.Escape(splitStr), RegexOptions.IgnoreCase);
            }
            else
            {
                return new string[0] { };
            }
        }
        /// <summary>
        /// 分割字符串数组
        /// </summary>
        /// <param name="sourceStr">要分割字符串</param>
        /// <param name="splitStr">分割字符</param>
        /// <param name="count">将从要分割字符串中分割出的字符数组最大索引数</param>
        /// <returns></returns>
        public static string[] Split(this string sourceStr, string splitStr, int count)
        {
            string[] result = new string[count];

            string[] splited = Split(sourceStr, splitStr);

            for (int i = 0; i < count; i++)
            {
                if (i < splited.Length)
                    result[i] = splited[i];
                else
                    result[i] = string.Empty;
            }

            return result;
        }
        #endregion

        /// <summary>
        /// 将字符串中的全角字符转换为半角
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <returns></returns>
        public static string ToBj(this string str)
        {
            if (str == null || str.Trim() == string.Empty) return str;

            StringBuilder sb = new StringBuilder(str.Length);
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '\u3000')
                    sb.Append('\u0020');
                else if (ValidationHelper.IsQjChar(str[i]))
                    sb.Append((char)((int)str[i] - 65248));
                else
                    sb.Append(str[i]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获得传入字符串的字节长度
        /// </summary>
        /// <param name="str">要获取的字符串</param>
        /// <returns></returns>
        /// <remarks>
        /// Utiltools 转来
        /// </remarks>
        public static int GetByteLength(this string str)
        {
            return Encoding.Default.GetByteCount(str);
        }

        /// <summary>
        /// 计算的字符串长度，并截取指定长度
        /// </summary>
        /// <param name="str">要获取的字符串</param>
        /// <param name="length">指定长度</param>
        /// <returns></returns>
        public static string SubByteString(this string str, int length)
        {
            int iByteCount;

            iByteCount = GetByteLength(str);
            if (iByteCount > length)
            {
                string sTemp;

                for (int i = length / 2; i <= str.Length; i++)
                {
                    sTemp = str.Substring(0, i);
                    if (GetByteLength(sTemp) > length)
                    {
                        str = sTemp.Substring(0, sTemp.Length - 1);
                        break;
                    }
                }
            }

            return str;
        }

        /// <summary>
        /// UTF8转换GB2312
        /// </summary>
        /// <param name="Str">要转换的字符串</param>
        /// <returns></returns>
        public static string UTF8ToGB2312(this string Str)
        {
            byte[] bt;
            bt = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(Str);
            bt = System.Text.Encoding.Convert(System.Text.Encoding.UTF8, System.Text.Encoding.GetEncoding("GB2312"), bt);
            return System.Text.Encoding.GetEncoding("GB2312").GetString(bt);
        }


        /// <summary>
        /// 按字节截取字符串长度
        /// </summary>
        /// <param name="str">要节截的字符串</param>
        /// <param name="length">指定长度</param>
        /// <returns></returns>
        public static string CutString(this string str, int length)
        {
            string sRet = string.Empty;
            if (!string.IsNullOrEmpty(str))
            {
                if (GetByteLength(str) > length * 2)
                {
                    sRet = SubByteString(str, length * 2) + "...";
                }
                else
                {
                    sRet = str;
                }
            }
            return sRet;
        }

        /// <summary>
        /// 截取字符串长度
        /// </summary>
        /// <param name="source">要节截的字符串</param>
        /// <param name="length">指定长度</param>
        /// <returns></returns>
        public static string CutUnicodeString(this string str, int length)
        {
            if (str.Length > length)
            {
                return str.Substring(0, length) + "...";
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// 转半角的函数(DBC case)
        /// </summary>
        /// <param name="input">任意字符串</param>
        /// <returns>半角字符串</returns>
        ///<remarks>
        ///全角空格为12288，半角空格为32
        ///其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        ///</remarks>
        public static string ToDBC(this string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new string(c);
        }

        /// <summary>
        /// 转换成指定类型
        /// </summary>
        /// <typeparam name="T">目标类型，只支持基础类型</typeparam>
        /// <param name="s">待转换的字符串</param>
        /// <param name="defaultValue">转换失败返回的默认值</param>
        /// <returns>转换成目标类型之后的值</returns>
        public static T As<T>(this string s, T defaultValue)
        {
            if (string.IsNullOrEmpty(s))
            {
                return defaultValue;
            }
            var type = typeof(T);
            try
            {
                T result;
                if (type.IsEnum)
                {
                    return (T)Enum.Parse(type, s, false);
                }
                var tryParse = TryParses.ContainsKey(type) ? TryParses[type] as TryParse<T> : GetTryPaseDelegate<T>();
                if (tryParse == null || !tryParse.Invoke(s, out result))
                {
                    return defaultValue;
                }
                if (!TryParses.ContainsKey(type))
                {
                    TryParses.Add(type, tryParse);
                }
                return result;
            }
            catch
            {
                return defaultValue;
            }
        }
        /// <summary>
        /// 根据指定类型获取TryParse的委托
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>对应类型的TryParse委托，null 表示没有对应的方法</returns>
        public static TryParse<T> GetTryPaseDelegate<T>()
        {
            var type = typeof(T);
            if (!HasTryParseMethod(type))
            {
                return null;
            }
            var method = GetMethods(type, "TryParse", BindingFlags.Public | BindingFlags.Static, new Type[] { typeof(string), typeof(T).MakeByRefType() });
            if (method == null)
            {
                return null;
            }
            var tempDelegate = Delegate.CreateDelegate(typeof(TryParse<T>), method, false);
            return tempDelegate as TryParse<T>;
        }

        /// <summary>
        /// 获取类型中的方法
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <param name="methodName">获取的方法</param>
        /// <param name="bindingAttr">搜索方法的方式</param>
        /// <param name="parameterTypes">方法的参数类型</param>
        /// <returns>
        /// 搜索到的方法对象
        /// <para>搜索不到返回null</para>
        /// </returns>
        public static MethodInfo GetMethods(Type type, string methodName, BindingFlags bindingAttr, Type[] parameterTypes = null)
        {
            if (type == null || methodName == null)
            {
                throw new ArgumentNullException(type == null ? "type" : "methodName");
            }
            var hasParameter = parameterTypes != null && parameterTypes.Length > 0;
            var methods = type.GetMethods(bindingAttr);
            foreach (var method in methods)
            {
                if (!method.Name.Equals(methodName))
                {
                    continue;
                }
                var parameters = method.GetParameters();
                var methodHasParameter = parameters.Length > 0;
                if (!hasParameter && !methodHasParameter)
                {
                    return method;
                }
                if ((hasParameter && !methodHasParameter) || (!hasParameter))
                {
                    continue;
                }
                var paramTypes = parameters.Select(paramter => paramter.ParameterType);
                if (paramTypes.Count() != parameterTypes.Count())
                {
                    continue;
                }
                int i = 0;
                foreach (var paramType in paramTypes)
                {
                    if (!(paramType == parameterTypes[i]))
                        break;
                    i++;
                }
                if (i >= paramTypes.Count())
                {
                    return method;
                }
            }
            return null;
        }
        /// <summary>
        /// 是否有定义TryParse方法
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        private static bool HasTryParseMethod(Type type)
        {
            var types = new[] { typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(short), typeof(ushort), typeof(float), 
                typeof(double), typeof(decimal), typeof(DateTime), typeof(byte), typeof(bool) };
            return types.Any(t => t == type);
        }
        /// <summary>   
        /// 类型转换委托
        /// </summary>
        /// <typeparam name="T">转换的目标类型，支持基础类型</typeparam>
        /// <param name="s">待转换的字符串</param>
        /// <param name="result">转换结果</param>
        /// <returns></returns>
        public delegate bool TryParse<T>(string s, out T result);


        public static bool EnsureNotNullOrEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        public static string WithFormat(this string str, params object[] args)
        {
            return string.Format(str, args);
        }

        public static string ToSqlString(this string str)
        {
            if (str == null)
                return null;
            if (str.IndexOf("'") != -1) //判断字符串是否含有单引号
            {
                str = str.Replace("'", "''");
            }
            return str;
        }

        public static int SafeToInt(this object obj)
        {
            int tenp;
            if (int.TryParse(obj.ToString(), out tenp))
            {
                return tenp;
            }
            return 0;
        }

        public static decimal SafeToDecimal(this object obj)
        {
            decimal temp;
            if (obj == null)
                return default(decimal);
            if (decimal.TryParse(obj.ToString(), out temp))
            {
                return temp;
            }
            return default(decimal);
        }

        public static T SafeConvertTo<T>(this object entity, T defaultValue = default(T))
        {
            if (entity == null)
                return defaultValue;
            try
            {
                return (T)Convert.ChangeType(entity, typeof(T));
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static bool SafeEquals(this string entity, string obj,
                                      StringComparison comparison = StringComparison.Ordinal)
        {
            if (entity == null)
            {
                return obj == null;
            }
            return entity.Equals(obj, comparison);
        }
    }
}
