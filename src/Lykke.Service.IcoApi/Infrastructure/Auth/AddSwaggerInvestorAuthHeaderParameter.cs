using System.Collections.Generic;
using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.IcoApi.Infrastructure.Auth
{
    public class AddSwaggerInvestorAuthHeaderParameter : IOperationFilter
    {
        void IOperationFilter.Apply(Operation operation, OperationFilterContext context)
        {
            var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
            var isAuthorized = filterPipeline.Select(f => f.Filter).Any(f => f is InvestorAuthAttribute);
            var authorizationRequired = context.ApiDescription.ControllerAttributes().Any(a => a is InvestorAuthAttribute);
            if (!authorizationRequired) authorizationRequired = context.ApiDescription.ActionAttributes().Any(a => a is InvestorAuthAttribute);

            if (isAuthorized && authorizationRequired)
            {
                if (operation.Parameters == null)
                    operation.Parameters = new List<IParameter>();

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "authToken",
                    In = "header",
                    Description = "Auth Token",
                    Required = true,
                    Type = "string"
                });
            }
        }
    }
}
