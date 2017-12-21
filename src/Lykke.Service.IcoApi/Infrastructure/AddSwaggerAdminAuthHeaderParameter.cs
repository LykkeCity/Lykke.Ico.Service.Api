using System.Collections.Generic;
using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.IcoApi.Infrastructure
{
    public class AddSwaggerAdminAuthHeaderParameter : IOperationFilter
    {
        void IOperationFilter.Apply(Operation operation, OperationFilterContext context)
        {
            var filterPipeline = context.ApiDescription.ActionDescriptor.FilterDescriptors;
            var isAuthorized = filterPipeline.Select(f => f.Filter).Any(f => f is AdminAuthAttribute);
            var authorizationRequired = context.ApiDescription.ControllerAttributes().Any(a => a is AdminAuthAttribute);
            if (!authorizationRequired) authorizationRequired = context.ApiDescription.ActionAttributes().Any(a => a is AdminAuthAttribute);

            if (isAuthorized && authorizationRequired)
            {
                if (operation.Parameters == null)
                    operation.Parameters = new List<IParameter>();

                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "adminAuthToken",
                    In = "header",
                    Description = "Admin Auth Token",
                    Required = true,
                    Type = "string"
                });
            }
        }
    }
}
