using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.Swagger
{
    public class IdempotencyHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var httpMethod = context.ApiDescription.HttpMethod?.ToUpperInvariant();
            if (httpMethod != "POST") return; 

            operation.Parameters ??= [];
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Idempotency-Key",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Optional idempotency key to make POST retries safe",
                Schema = new OpenApiSchema { Type = "string" }
            });
        }
    }
}
