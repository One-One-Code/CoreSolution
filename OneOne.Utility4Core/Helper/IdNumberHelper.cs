using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace OneOne.Utility4Core.Helper
{
    /// <summary>
    /// 身份证号码帮助类
    /// </summary>
    public class IdNumberHelper
    {
        /// <summary>
        /// 身份证最后一位的校验码
        /// </summary>
        private static string[] IdNumberCheckMapping = new string[11] { "1", "0", "X", "9", "8", "7", "6", "5", "4", "3", "2" };

        /// <summary>
        /// 校验身份证是否合法
        /// 参照GB11643-1999标准
        /// </summary>
        /// <param name="number">18位的身份证号码</param>
        /// <returns></returns>
        public static bool CheckIdNumber(string number)
        {
            if (string.IsNullOrEmpty(number) || number.Length != 18)
            {
                return false;
            }
            var regxString = @"^\d{6}(18|19|20)\d{2}((0[1-9])|(1[0-2]))(([0-2][1-9])|10|20|30|31)\d{3}[0-9Xx]$";
            var regex = new Regex(regxString);
            var regxCheck = regex.IsMatch(number);
            if (regxCheck)
            {
                var sum = 0;
                for (var i = 0; i < 17; i++)
                {
                    var w = (int)Math.Pow(2, i + 1) % 11;
                    sum += w * Convert.ToInt32(number[18 - i - 2].ToString());
                }
                var checkNumber = sum % 11;
                var text = IdNumberCheckMapping[checkNumber].ToString();
                if (text.Equals(number[17].ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 校验身份证是否合法，同时校验男女
        /// 参照GB11643-1999标准
        /// </summary>
        /// <param name="number">18位的身份证号码</param>
        /// <param name="man">性别，true标识男性，false标识女性</param>
        /// <returns></returns>
        public static bool CheckIdNumber(string number, bool man)
        {
            var baseCheck = CheckIdNumber(number);
            if (baseCheck)
            {
                var sexnumber = Convert.ToInt32(number[16].ToString());
                if ((sexnumber % 2 != 0) == man)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
