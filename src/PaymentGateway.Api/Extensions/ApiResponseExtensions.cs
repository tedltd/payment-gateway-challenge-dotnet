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
                HttpStatusCode.BadRequest => new BadRequestObjectResult(response.Payload),
                HttpStatusCode.PaymentRequired => new ObjectResult(response.Payload) { StatusCode = 402 },
                HttpStatusCode.RequestTimeout => new ObjectResult(response.Payload) { StatusCode = 408 },
                HttpStatusCode.ServiceUnavailable => new ObjectResult(response.Payload) { StatusCode = 503 },
                HttpStatusCode.GatewayTimeout => new ObjectResult(response.Payload) { StatusCode = 504 },
                _ => new ObjectResult(response.Payload) { StatusCode = 500 }
            };
        }
    }
}
