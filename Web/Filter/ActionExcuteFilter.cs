using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Filter
{
    public class ActionExcuteFilter : IActionFilter, IExceptionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            
            throw new NotImplementedException();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
         
            throw new NotImplementedException();
        }

        public void OnException(ExceptionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
