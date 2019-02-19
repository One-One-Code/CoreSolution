using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CommonService;
using OneOne.Core.Logger;
using log4net;
using log4net.Repository;
using log4net.Config;
using Microsoft.AspNetCore.Authorization;
using OneOne.Utility4Core.Helper;
using Web.Common;

namespace Web.Controllers
{
    
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        //[Authorize("UserBind")]
        public OmcConfigView Get(int id)
        {
            Log.Write("请求get方法", MessageType.Info);
            Log.Write("请求get方法", MessageType.Warn);
            //var service = new CommonServiceClient();
            //var result = service.GetOmcConfigAsync().Result;

            //return result;
            var token = JwtHelper.GetJwt(11);
            var userId = JwtHelper.JwtValidate(token);
            return new OmcConfigView() { ProductCode= JwtHelper.GetJwt(11) };
        }

        // POST api/values
        [HttpPost]
        //[Authorize("UserBind")]
        public void Post([FromBody]string value)
        {
        }

        [Authorize(Policy = "jwt")]
        [HttpPost]
        public DoRequest Do([FromBody]DoRequest request)
        {
            var userId = HttpContext.GetUserId();
            request.age = userId;
            return request;
        }

        [HttpGet]
        public FileResult getcodeimage()
        {
            var bytes = ValidateCodeHelper.CreateValidateGraphic("4506");
            return new FileContentResult(bytes, @"image/jpeg");
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }

    public class DoRequest
    {
        public string name { get; set; }

        public int age { get; set; }
    }
}
