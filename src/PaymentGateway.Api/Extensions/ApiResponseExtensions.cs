using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Domain.Response;
using System.Net;

namespace PaymentGateway.Api.Extensions
{
    public static class ApiResponseExtensions
    {
        public static IActionResult ToActionResult<T>(this ApiResponse<T> response)
        {
            var errorResponse = new { error = response.ErrorMessage, message = response.Message };

            return response.StatusCode switch
            {
                HttpStatusCode.OK => new OkObjectResult(response.Payload),
                HttpStatusCode.BadRequest => new BadRequestObjectResult(errorResponse),
                HttpStatusCode.PaymentRequired => new ObjectResult(errorResponse) { StatusCode = 402 },
                HttpStatusCode.RequestTimeout => new ObjectResult(errorResponse) { StatusCode = 408 },
                HttpStatusCode.ServiceUnavailable => new ObjectResult(errorResponse) { StatusCode = 503 },
                HttpStatusCode.GatewayTimeout => new ObjectResult(errorResponse) { StatusCode = 504 },
                _ => new ObjectResult(errorResponse) { StatusCode = 500 }
            };
        }
    }
}
