using System.Text.Json;

namespace PaymentGateway.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // all the error messages should be codes and we should add localisation , but not required for this solution

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred while processing the request. Path: {Path}, Method: {Method}",
                    context.Request.Path, context.Request.Method);
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                ArgumentNullException => (StatusCodes.Status400BadRequest, "A required argument was not provided."),
                ArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument provided."),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized access."),
                NotImplementedException => (StatusCodes.Status501NotImplemented, "Feature not implemented."),
                TimeoutException => (StatusCodes.Status408RequestTimeout, "The request timed out."),
                _ => (StatusCodes.Status500InternalServerError, "An internal server error occurred.")
            };

            context.Response.StatusCode = statusCode;

            var response = new
            {
                error = message,
                statusCode,
                timestamp = DateTime.UtcNow
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options)).ConfigureAwait(false);
        }
    }
}
