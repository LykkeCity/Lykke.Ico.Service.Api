using System.Collections.Generic;
using System.Linq;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;

namespace Lykke.Service.IcoApi.Infrastructure.Auth
{
    public class AddSwaggerAuthorizationHeaderParameter : IOperationFilter
    {
        void IOperationFilter.Apply(Operation operation, OperationFilterContext context)
        {
            var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
            var isAuthorized = filterPipeline.Select(f => f.Filter).Any(f => f is UserAuthAttribute);
            var authorizationRequired = context.ApiDescription.GetControllerAttributes().Any(a => a is UserAuthAttribute);
            if (!authorizationRequired) authorizationRequired = context.ApiDescription.GetActionAttributes().Any(a => a is UserAuthAttribute);

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
