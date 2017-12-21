using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Lykke.Service.IcoApi.Core.Settings.ServiceSettings;
using System.Net;

namespace Lykke.Service.IcoApi.Infrastructure
{
    public class DisableWhenOnProdAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var apiSettings = context.HttpContext.RequestServices.GetService<IcoApiSettings>();

            if (apiSettings.DisableAdminDebugMethods)
            {
                context.Result = new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }

            base.OnActionExecuting(context);
        }
    }
}
