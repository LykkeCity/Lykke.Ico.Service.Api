using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Lykke.Service.IcoApi.Infrastructure.Auth
{
    public class AdminAuthAttribute : ActionFilterAttribute
    {
        private readonly string HeaderName = "adminAuthToken";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var key = "";

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

            if (!"TEMP".Equals(key))
            {
                context.Result = new UnauthorizedResult();
            }

            base.OnActionExecuting(context);
        }
    }
}
