using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Lykke.Service.IcoApi.Core.Settings.ServiceSettings;
using System.Net;

namespace Lykke.Service.IcoApi.Infrastructure
{
    public class DisableAdminMethodsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var apiSettings = context.HttpContext.RequestServices.GetService<IcoApiSettings>();

            if (apiSettings.DisableAdminMethods)
            {
                context.Result = new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }

            base.OnActionExecuting(context);
        }
    }
}
