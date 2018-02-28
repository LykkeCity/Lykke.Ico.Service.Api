using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Lykke.Service.IcoApi.Core.Settings.ServiceSettings;
using Lykke.Service.IcoApi.Core.Services;
using System.Threading.Tasks;

namespace Lykke.Service.IcoApi.Infrastructure
{
    public class AdminAuthAttribute : ActionFilterAttribute
    {
        private readonly string HeaderName = "adminAuthToken";

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var key = "";

            var apiSettings = context.HttpContext.RequestServices.GetService<IcoApiSettings>();

            var authService = context.HttpContext.RequestServices.GetService<IAuthService>();

            if (context.HttpContext.Request.Headers.ContainsKey(HeaderName))
            {
                var headers = context.HttpContext.Request.Headers[HeaderName];
                key = headers[0];
            }

            if (string.IsNullOrEmpty(key) &&
                context.HttpContext.Request.HasFormContentType &&
                context.HttpContext.Request.Form.ContainsKey(HeaderName))
            {
                var forms = context.HttpContext.Request.Form[HeaderName];
                key = forms[0];
            }

            if (!apiSettings.AdminAuthKey.Equals(key) && !(await authService.IsValid(key)))
            {
                context.Result = new UnauthorizedResult();
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
