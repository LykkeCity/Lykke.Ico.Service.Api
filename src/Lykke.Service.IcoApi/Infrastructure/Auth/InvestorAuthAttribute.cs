using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Lykke.Service.IcoApi.Infrastructure.Auth
{
    public class InvestorAuthAttribute : ActionFilterAttribute
    {
        private readonly string HeaderName = "authToken";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.Headers.ContainsKey(HeaderName))
            {
                context.Result = new UnauthorizedResult();
            }
            else
            {
                var apiKeyFromRequest = context.HttpContext.Request.Headers[HeaderName];

                //TODO - add validation of token
                if (!"TEMP".Equals(apiKeyFromRequest[0]))
                {
                    context.Result = new UnauthorizedResult();
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
