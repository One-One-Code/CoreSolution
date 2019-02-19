using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Common
{
    public static class ControllerHelper
    {
        /// <summary>
        /// 获取当前请求的userid
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static int GetUserId(this HttpContext httpContext)
        {
            int userId = 0;
            var user = httpContext.User.FindFirst(p => p.Type.Equals("UserId"));
            if (!string.IsNullOrEmpty(user.Value))
            {
                Int32.TryParse(user.Value,out userId);
            }
            return userId;
        }
    }
}
