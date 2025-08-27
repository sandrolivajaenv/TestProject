using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data.Common;
using System.Globalization;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.RateLimiting;

namespace API.Tests
{
    public class CustomWebAppFactory : WebApplicationFactory<Program>
    {
        private DbConnection? _connection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(_connection));

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();

                services.AddRateLimiter(options =>
                {
                    // 2 requests per 10 seconds globally (same bucket for all requests)
                    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: "global-test",
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 5,
                                Window = TimeSpan.FromSeconds(10),
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = 0
                            }));

                    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                    options.OnRejected = async (context, token) =>
                    {
                        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                        {
                            context.HttpContext.Response.Headers.RetryAfter =
                                ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            context.HttpContext.Response.Headers.RetryAfter = "10";
                        }

                        await context.HttpContext.Response
                            .WriteAsync("Too many requests. Please retry later.", token);
                    };
                });
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _connection?.Dispose();
        }
    }

    sealed class TimeProviderClock(TimeProvider tp) : ISystemClock
    {
        private readonly TimeProvider _tp = tp;

        public DateTimeOffset UtcNow => _tp.GetUtcNow();
    }

    public class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        TimeProvider timeProvider) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder, new TimeProviderClock(timeProvider))
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = new ClaimsIdentity(
            [
            new Claim(ClaimTypes.Name, "test-user"),
            new Claim(ClaimTypes.Role, "Admin")
        ], "Test");

            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}