using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace API.Tests
{
    public class RateLimitingTests : IClassFixture<CustomWebAppFactory>
    {
        private readonly HttpClient _client;

        public RateLimitingTests(CustomWebAppFactory factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task Exceeding_global_rate_limit_returns_429()
        {
            // Hit an endpoint (allow-anonymous is fine). Use something cheap like GET products.
            var r1 = await _client.GetAsync("/api/v1/products?page=1&pageSize=1");
            r1.StatusCode.Should().Be(HttpStatusCode.OK);

            var r2 = await _client.GetAsync("/api/v1/products?page=1&pageSize=1");
            r2.StatusCode.Should().Be(HttpStatusCode.OK);
            var r3 = await _client.GetAsync("/api/v1/products?page=1&pageSize=1");
            r3.StatusCode.Should().Be(HttpStatusCode.OK);
            var r4 = await _client.GetAsync("/api/v1/products?page=1&pageSize=1");
            r4.StatusCode.Should().Be(HttpStatusCode.OK);
            var r5 = await _client.GetAsync("/api/v1/products?page=1&pageSize=1");
            r5.StatusCode.Should().Be(HttpStatusCode.OK);

            // 6th request within the same 10s window should be throttled
            var r6 = await _client.GetAsync("/api/v1/products?page=1&pageSize=1");
            r6.StatusCode.Should().Be((HttpStatusCode)429);

            //  Retry-After is commonly set, assert if present)
            var hasRetryAfter = r6.Headers.TryGetValues("Retry-After", out var _);
            hasRetryAfter.Should().BeTrue("rate limiter should advise when to retry");
        }
    }
}