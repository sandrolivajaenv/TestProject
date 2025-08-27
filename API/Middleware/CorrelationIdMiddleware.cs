namespace API.Middleware
{
    public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        public const string HeaderName = "X-Correlation-Id";
        public async Task Invoke(HttpContext ctx)
        {
            var cid = ctx.Request.Headers.TryGetValue(HeaderName, out var v) && !string.IsNullOrWhiteSpace(v)
                ? v.ToString()
                : Guid.NewGuid().ToString("N");

            ctx.Response.Headers[HeaderName] = cid;

            using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = cid }))
            {
                await next(ctx);
            }
        }
    }
}
