using FluentValidation;

using PaymentGateway.Api.Extensions;
using PaymentGateway.Api.Services;
using PaymentGateway.Domain.Request;

using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PaymentGateway.Api.Middleware
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidationMiddleware> _logger;
        //private static readonly Regex PaymentPostRoutePattern = new(@"^/api/(v\d+/)?payment$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        //private static readonly Regex PaymentGetRoutePattern = new(@"^/api/(v\d+/)?payment/[a-fA-F0-9-]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // all error messages should be a code, and use locale really
        public ValidationMiddleware(RequestDelegate next, ILogger<ValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, PaymentMetrics metrics)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var method = context.Request.Method;

            //if (PaymentPostRoutePattern.IsMatch(path) && method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            if (method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                await ValidatePostRequest(context, metrics);
                return;
            }

            if (method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                await ValidateGetRequest(context, metrics);
                return;
            }
            await _next(context).ConfigureAwait(false);
        }

        private async Task ValidatePostRequest(HttpContext context, PaymentMetrics metrics)
        {
            try
            {
                context.Request.EnableBuffering();

                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
                var body = await reader.ReadToEndAsync().ConfigureAwait(false);
                context.Request.Body.Position = 0;

                if (string.IsNullOrWhiteSpace(body))
                {
                    _logger.LogWarning("Validation failed: Request body is empty");
                    metrics.RecordValidationFailure("RequestBody");

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { errors = new[] { "Request body is required." } });
                    return;
                }

                PaymentRequest? request;
                try
                {
                    var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    request = JsonSerializer.Deserialize<PaymentRequest>(body, opts);
                    if (request is null)
                    {
                        _logger.LogWarning("Validation failed: Unable to deserialize request body");
                        metrics.RecordValidationFailure("Deserialization");

                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(new { errors = new[] { "Invalid JSON payload." } });
                        return;
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Validation failed: JSON deserialization error");
                    metrics.RecordValidationFailure("JsonFormat");

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { errors = new[] { "Invalid JSON payload." } });
                    return;
                }

                var validator = context.RequestServices.GetRequiredService<IValidator<PaymentRequest>>();
                var validationResult = await validator.ValidateAsync(request).ConfigureAwait(false);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Validation failed for payment request. Errors: {ErrorCount}", validationResult.Errors.Count);

                    foreach (var error in validationResult.Errors)
                    {
                        metrics.RecordValidationFailure(error.PropertyName);
                    }

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;

                    var errors = validationResult.Errors
                        .GroupBy(e => StringExtensions.ToCamelCase(e.PropertyName))
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => new { message = e.ErrorMessage, code = e.ErrorCode, metadata = e.CustomState }).ToArray()
                        );

                    await context.Response.WriteAsJsonAsync(new { errors });
                    return;
                }

                await _next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during POST validation middleware processing");
                throw;
            }
        }

        private async Task ValidateGetRequest(HttpContext context, PaymentMetrics metrics)
        {
            try
            {
                var pathSegments = context.Request.Path.Value?.Split('/');
                var idSegment = pathSegments?.LastOrDefault();

                if (string.IsNullOrWhiteSpace(idSegment))
                {
                    _logger.LogWarning("Validation failed: Payment ID is missing");
                    metrics.RecordValidationFailure("PaymentId");

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { errors = new[] { "Payment ID is required." } });
                    return;
                }

                if (!Guid.TryParse(idSegment, out _))
                {
                    _logger.LogWarning("Validation failed: Payment ID is not a valid GUID. Value: {Id}", idSegment);
                    metrics.RecordValidationFailure("PaymentId");

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { errors = new[] { "Payment ID must be a valid GUID." } });
                    return;
                }

                _logger.LogDebug("GET request validation passed for payment ID: {PaymentId}", idSegment);
                await _next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during GET validation middleware processing");
                throw;
            }
        }
    }
}