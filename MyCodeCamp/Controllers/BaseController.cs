using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MyCodeCamp.Controllers
{
    public abstract class BaseController:Controller
    {
        public const string URL_HELPER = "URL_HELPER";

        // Called before Action method is executed
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            // Provide access to the Url Helper
            context.HttpContext.Items[URL_HELPER] = this.Url;
        }
    }
}
