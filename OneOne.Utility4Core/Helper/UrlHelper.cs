using System.Web;
using System.Text;

namespace OneOne.Utility4Core.Helper
{
    /// <summary>
    /// URL相关
    /// </summary>
    public class UrlHelper
    {

        #region URL 编/解码 
        /// <summary>
        /// 使用UTF-8编码对URL编码，对URL参数编码时请一定使用此函数
        /// <remqrk>
        /// HttpUtility.UrlEncode内部默认使用UTF-8编码，为支持中文，在 Encode 的时候， 
        /// 将空格转换成加号('+'), 在 Decode 的时候将加号转为空格
        /// </remqrk>
        /// </summary>
        /// <param name="str">要编码的URL字符串</param>
        /// <returns>编码后的URL字符串</returns>
        public static string UrlEncode(string str)
        {
            //return HttpUtility.UrlEncode(str).Replace("+", "%20");
            return UrlEncode(str, null);
        }
        /// <summary>
        /// 使用指定字符编码对URL编码
        /// </summary>
        /// <param name="paramName">URL参数</param>
        /// <param name="encoding">字符编码</param>
        /// <returns>编码后的URL参数</returns>
        public static string UrlEncode(string str, Encoding encoding)
        {
            if (encoding == null || encoding == Encoding.UTF8) 
            {
                //return UrlEncode(str);
                return HttpUtility.UrlEncode(str).Replace("+", "%20");
            }
            else
            {
                return HttpUtility.UrlEncode(str, encoding);
            }
        }
        /// <summary>
        /// 使用UTF-8编码对URL解码，对URL参数解码时请一定使用此函数
        /// <remqrk>
        /// HttpUtility.UrlEncode内部默认使用UTF-8编码，为支持中文，在 Encode 的时候，
        /// 将空格转换成加号('+'), 在 Decode 的时候将加号转为空格
        /// </remqrk>
        /// </summary>
        /// <param name="str">要解码的URL字符串</param>
        /// <returns>解码后的URL字符串</returns>
        public static string UrlDecode(string str)
        {
            return HttpUtility.UrlDecode(str);
        }
        /// <summary>
        /// 使用指定字符编码对URL解码
        /// </summary>
        /// <param name="paramName">URL参数</param>
        /// <param name="encoding">字符编码</param>
        /// <returns>解码后的URL参数</returns>
        public static string UrlDecode(string str, Encoding encoding)
        {
            return HttpUtility.UrlDecode(str, encoding);
        }
        #endregion

    }
}
