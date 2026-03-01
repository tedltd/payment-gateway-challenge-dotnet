using Microsoft.AspNetCore.Authorization;
using PaymentGateway.Api.Attributes;
using PaymentGateway.Application.Services;

namespace PaymentGateway.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class RequireAuthorizationAttribute : Attribute
    {
    }
}

namespace PaymentGateway.Api.Middleware
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string? _expectedApiKey;

        public AuthorizationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _expectedApiKey = configuration["ApiKey"];
        }

        public async Task InvokeAsync(HttpContext context, PaymentMetricsService metrics)
        {
            var endpoint = context.GetEndpoint();

            // If endpoint explicitly allows anonymous, skip authorization , only for this demo obviously
            // we could of course use the APIM to authorise our endpoints - preferred option

            var allowAnonymous = endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null;
            if (allowAnonymous)
            {
                await _next(context).ConfigureAwait(false);
                return;
            }

            var requiresAuth = endpoint?.Metadata?.GetMetadata<RequireAuthorizationAttribute>() != null;

            if (!requiresAuth)
            {
                await _next(context).ConfigureAwait(false);
                return;
            }

            if (!context.Request.Headers.TryGetValue("Authorization", out var headerValue) ||
                string.IsNullOrWhiteSpace(headerValue))
            {
                metrics.RecordAuthorizationFailure("MissingHeader");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" }).ConfigureAwait(false);
                return;
            }

            if (!string.IsNullOrEmpty(_expectedApiKey))
            {
                
                if (!headerValue.ToString().Contains(_expectedApiKey))
                {
                    metrics.RecordAuthorizationFailure("InvalidApiKey");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" }).ConfigureAwait(false);
                    return;
                }
            }

            await _next(context).ConfigureAwait(false);
        }
    }
}