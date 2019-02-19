using HandshakeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Web.Models;

namespace Web.Filter
{
    public class UserBindAuthorizeHandler : AuthorizationHandler<UserBindAuthorizeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserBindAuthorizeRequirement requirement)
        {
            //if (CheckPartner(context))
            //{
            //    context.Succeed(requirement);
            //}
            //else
            //{
            //    HandleUnauthorizedRequest(context);
            //}
            HandleUnauthorizedRequest(context);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 获取请求里面供验证的数据
        /// </summary>
        /// <returns>供验证的数据对象</returns>
        private PartnerRequestBase GetRequestBase(AuthorizationFilterContext context)
        {

            var bodyStr = "";
            var req = context.HttpContext.Request;

            // Allows using several time the stream in ASP.Net Core
            req.EnableRewind();

            // Arguments: Stream, Encoding, detect encoding, buffer size 
            // AND, the most important: keep stream opened
            using (StreamReader reader
                      = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
            {
                bodyStr = reader.ReadToEnd();
            }

            // Rewind, so the core is not lost when it looks the body for the request
            req.Body.Position = 0;
            var result = JsonConvert.DeserializeObject(bodyStr);
            if (result == null)
            {
                return null;
            }
            var baseEntity = JsonConvert.DeserializeObject<PartnerRequestBase>(result.ToString());
            return baseEntity;
        }

        protected bool CheckPartner(AuthorizationHandlerContext context)
        {
            var filterContext = (context.Resource as AuthorizationFilterContext);
            var appInfo = this.GetRequestBase(filterContext);
            var partner = PartnerKeyAndSecretValidation(appInfo.AppKey, appInfo.AppSecret);
            if (partner == null)
            {
                return false;
            }
            filterContext.HttpContext.Request.Headers.Add("PartnerId", partner.PartnerId.ToString());
            filterContext.HttpContext.Request.Headers.Add("PartnerToken", partner.PartnerToken);
            filterContext.HttpContext.Request.Headers.Add("PartnerTypeId", partner.PartnerTypeId.ToString());
            return true;
        }

        /// <summary>
        /// 校验PartnerKey和Secret
        /// </summary>
        /// <param name="appKey">appKey</param>
        /// <param name="appSecret">appSecret</param>
        /// <returns>true/false</returns>
        protected Partner PartnerKeyAndSecretValidation(string appKey, string appSecret)
        {
            if (string.IsNullOrEmpty(appKey) || string.IsNullOrEmpty(appSecret))
            {
                return null;
            }
            var handshakeService = new HandshakeServiceClient();
            var partner = handshakeService.GetPartnerAsync(appKey).Result;
            return partner != null && partner.StatusFlag == 1 && partner.PartnerSecret == appSecret ? partner : null;
        }

        protected void HandleUnauthorizedRequest(AuthorizationHandlerContext context)
        {
            //(context.Resource as AuthorizationFilterContext).HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            var obj = ApiResultObj.ReturnExpResult("AppKey或AppSecret错误");
            var memory = new MemoryStream();
            using (StreamWriter writer
                      = new StreamWriter(memory))
            {
                writer.Write(obj);
            }
            (context.Resource as AuthorizationFilterContext).HttpContext.Response.Body = memory;
            //context.
        }
    }
}
