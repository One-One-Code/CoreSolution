using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Models
{
    public class ApiResultObj
    {
        /// <summary>
        /// Api状态码
        /// </summary>
        public ApiResultType ApiResultType { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string ResultMessage { get; set; }

        /// <summary>
        /// Api的返回值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 返回正常结果
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="value">返回值</param>
        /// <returns>返回异常结果</returns>
        public static ApiResultObj ReturnOkResult(string msg, object value = null)
        {
            return new ApiResultObj
            {
                ApiResultType = ApiResultType.Success,
                ResultMessage = msg,
                Value = value
            };
        }

        /// <summary>
        /// 返回失败结果
        /// </summary>
        /// <param name="statusCode">状态码</param>
        /// <param name="msg">消息</param>
        /// <param name="value">返回值</param>
        /// <returns>返回失败结果</returns>
        public static ApiResultObj ReturnExpResult(string msg, object value = null)
        {
            return new ApiResultObj
            {
                ApiResultType = ApiResultType.Failed,
                ResultMessage = msg,
                Value = value
            };
        }
    }
}
