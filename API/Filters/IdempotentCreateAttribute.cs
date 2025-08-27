using API.Idempotency;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IdempotentCreateAttribute(string resourceName = "resource") : Attribute, IAsyncResourceFilter
{
    private readonly string _resourceName = resourceName;

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var http = context.HttpContext;
        var req = http.Request;

        if (!HttpMethods.IsPost(req.Method))
        {
            await next();
            return;
        }

        req.EnableBuffering();

        var reqHash = await IdempotencyHelpers.ComputeStableRequestHashAsync(req);

        if (!req.Headers.TryGetValue("Idempotency-Key", out var keyValues) ||
            string.IsNullOrWhiteSpace(keyValues.ToString()))
        {
            await next();
            return;
        }

        var key = keyValues.ToString();
        var store = http.RequestServices.GetRequiredService<IIdempotencyStore>();


        if (store.TryGet($"{req.Path}:{key}", out var record))
        {
            if (!string.Equals(record.RequestHash, reqHash, StringComparison.Ordinal))
            {
                context.Result = new ConflictObjectResult(new
                {
                    error = "Idempotency key reuse with different payload.",
                    key,
                    existingResourceId = record.ResourceId
                });
                return;
            }

            var version = http.GetRequestedApiVersion()?.ToString();
            context.Result = new CreatedAtActionResult(
                actionName: "Get",
                controllerName: context.ActionDescriptor.RouteValues["controller"],
                routeValues: new { version, productId = context.RouteData.Values["productId"], id = record.ResourceId },
                value: new { id = record.ResourceId }
            );
            return;
        }

        var executed = await next();

        if (executed.Result is CreatedAtActionResult created &&
            created.RouteValues is not null &&
            created.RouteValues.TryGetValue("id", out var idObj) &&
            idObj is int idVal)
        {
            store.Set(
                key: $"{req.Path}:{key}",
                record: new IdempotencyRecord(reqHash, idVal, DateTime.UtcNow),
                ttl: TimeSpan.FromHours(24));
        }
    }
}