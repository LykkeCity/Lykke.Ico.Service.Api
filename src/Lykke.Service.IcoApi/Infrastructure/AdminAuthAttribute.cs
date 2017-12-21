using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Lykke.Service.IcoApi.Core.Settings.ServiceSettings;

namespace Lykke.Service.IcoApi.Infrastructure
{
    public class AdminAuthAttribute : ActionFilterAttribute
    {
        private readonly string HeaderName = "adminAuthToken";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var key = "";

            var apiSettings = context.HttpContext.RequestServices.GetService<IcoApiSettings>();

            if (context.HttpContext.Request.Headers.ContainsKey(HeaderName))
            {
                var headers = context.HttpContext.Request.Headers[HeaderName];
                key = headers[0];
            }

            if (string.IsNullOrEmpty(key) &&
                context.HttpContext.Request.Form.ContainsKey(HeaderName))
            {
                var forms = context.HttpContext.Request.Form[HeaderName];
                key = forms[0];
            }

            if (!apiSettings.AdminAuthKey.Equals(key))
            {
                context.Result = new UnauthorizedResult();
            }

            base.OnActionExecuting(context);
        }
    }
}
