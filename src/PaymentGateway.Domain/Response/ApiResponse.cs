using System.Net;

namespace PaymentGateway.Domain.Response
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
        public T? Payload { get; set; }
        public HttpStatusCode? StatusCode { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(T payload, string message = "", HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            Success = true;
            Payload = payload;
            Message = message;
            StatusCode = statusCode;
        }
    }
}
