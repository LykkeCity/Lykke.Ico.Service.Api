﻿using Lykke.Ico.Core.Repositories.InvestorAttribute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;

namespace Lykke.Service.IcoApi.Infrastructure
{
    public class InvestorAuthAttribute : TypeFilterAttribute
    {
        public InvestorAuthAttribute() : base(typeof (InvestorAuthAttributeImpl))
        {

        }

        private class InvestorAuthAttributeImpl : IAuthorizationFilter
        {
            private readonly IInvestorAttributeRepository _investorAttributeRepository;
            private readonly string _headerName = "authToken";

            public InvestorAuthAttributeImpl(IInvestorAttributeRepository investorAttributeRepository)
            {
                _investorAttributeRepository = investorAttributeRepository;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (context.HttpContext.Request.Headers.ContainsKey(_headerName))
                {
                    var apiKeyFromRequest = context.HttpContext.Request.Headers[_headerName];
                    if (Guid.TryParse(apiKeyFromRequest, out var token))
                    {
                        var email = _investorAttributeRepository.GetInvestorEmailAsync(InvestorAttributeType.ConfirmationToken, token.ToString()).Result;
                        if (!string.IsNullOrEmpty(email))
                        {
                            var claims = new[] { new Claim(ClaimTypes.Email, email) };
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
