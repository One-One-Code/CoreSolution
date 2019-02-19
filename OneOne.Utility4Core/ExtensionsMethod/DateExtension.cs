using System;

namespace OneOne.Utility4Core.ExtensionsMethod
{
    /// <summary>
    /// 时间日期扩展方法
    /// </summary>
    public static class DateExtension
    {
        /// <summary>
        /// 返回传入参数年月的字符串，格式为 “YYYY-MM”
        /// </summary>
        /// <param name="date">当前操作的日期</param>
        /// <returns>年月的日期,格式为 “YYYY-MM”</returns>
        public static string GetMonthString(this DateTime date)
        {
            return date.Year.ToString() + "-" + date.Month.ToString();
        }

        /// <summary>
        /// 返回表示当前时间的字符串，格式为 HH:MM:SS
        /// </summary>
        /// <param name="time">当前操作的日期</param>
        /// <returns>时间的日期,格式为HH:MM:SS</returns>
        public static string GetTimeString(this DateTime time)
        {
            return time.Hour.ToString() + ":" + time.Minute.ToString().PadLeft(2, '0') + ":" + time.Second.ToString().PadLeft(2, '0');
        }

        /// <summary>
        /// 返回表示当前日期的字符串，格式为 YYYY-MM-DD
        /// </summary>
        /// <param name="date">当前操作的日期</param>
        /// <returns>日期,格式为YYYY-MM-DD</returns>
        public static string GetDateString(this DateTime date)
        {
            return date.Year.ToString() + "-" + date.Month.ToString().PadLeft(2, '0') + "-" + date.Day.ToString().PadLeft(2, '0');
        }

        /// <summary>
        /// 返回表示当前时间的字符串，格式为 YYYY-MM-DD HH:MM:SS
        /// </summary>
        /// <param name="time">当前操作的日期</param>
        /// <returns>日期,格式为YYYY-MM-DD HH:MM:SS</returns>
        public static string GetFullTimeString(this DateTime time)
        {
            return GetDateString(time) + " " + GetTimeString(time);
        }

        /// <summary>
        /// 得到两日期天数差值
        /// </summary>
        /// <param name="sDate">开始日期</param>
        /// <param name="eDate">结束日期</param>
        /// <returns>两日期天数差值</returns>
        public static int GetDateDiff(this System.DateTime sDate, System.DateTime eDate)
        {
            string days = "0";
            days = Convert.ToString(eDate.Subtract(sDate).Days);
            return int.Parse(days);
        }
    }
}
