using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Web.Common;

namespace Web.Filter
{
    public class JwtHandler : AuthorizationHandler<JwtRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, JwtRequirement requirement)
        {
            var filterContext = (context.Resource as AuthorizationFilterContext);
            StringValues authHeaderValue;
            filterContext.HttpContext.Request.Headers.TryGetValue("Authorization", out authHeaderValue);
            if (authHeaderValue.Count != 0)
            {
                var token = authHeaderValue[0];
                var userId = JwtHelper.JwtValidate(token);
                if (userId != 0)
                {
                    var claims = new List<Claim>();
                    claims.Add(new Claim("UserId", userId.ToString()));
                    filterContext.HttpContext.User.AddIdentity(new ClaimsIdentity(claims));
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// 请求的token校验，并校验用户是否有效
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private int ValidateToken(AuthorizationHandlerContext context)
        {
            var filterContext = (context.Resource as AuthorizationFilterContext);
            StringValues authHeaderValue;
            filterContext.HttpContext.Request.Headers.TryGetValue("Authorization", out authHeaderValue);
            var token = authHeaderValue[0];
            var userId = JwtHelper.JwtValidate(token);
            if (userId == 0)
            {
                return userId;
            }
            //TODO 校验用户信息，不成功则返回0
            return userId;
        }
    }
}
