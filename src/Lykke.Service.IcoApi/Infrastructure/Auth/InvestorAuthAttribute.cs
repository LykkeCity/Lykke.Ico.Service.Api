using Common.Log;
using Lykke.Service.IcoApi.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;
using System.Security.Principal;

namespace Lykke.Service.IcoApi.Infrastructure.Auth
{
    public class InvestorAuthAttribute : TypeFilterAttribute
    {
        public InvestorAuthAttribute() : base(typeof (InvestorAuthAttributeImpl))
        {

        }

        private class InvestorAuthAttributeImpl : IAuthorizationFilter
        {
            private readonly IInvestorService _investorService;
            private readonly string HeaderName = "authToken";

            public InvestorAuthAttributeImpl(IInvestorService investorService)
            {
                _investorService = investorService;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (context.HttpContext.Request.Headers.ContainsKey(HeaderName))
                {
                    var apiKeyFromRequest = context.HttpContext.Request.Headers[HeaderName];
                    if (Guid.TryParse(apiKeyFromRequest, out var token))
                    {
                        var confirmation = _investorService.GetConfirmation(token).Result;
                        if (confirmation != null)
                        {
                            var claims = new[] { new Claim(ClaimTypes.Email, confirmation.Email) };
                            var identity = new ClaimsIdentity(claims);

                            context.HttpContext.User = new ClaimsPrincipal(identity);

                            return;
                        }
                    }
                }

                context.Result = new UnauthorizedResult();
            }
        }
    }
}
