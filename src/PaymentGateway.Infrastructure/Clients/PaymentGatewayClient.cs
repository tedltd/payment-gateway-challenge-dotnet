using System.Net.Http.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Models;
using PaymentGateway.Domain.Response;
using PaymentGateway.Infrastructure.Options;

namespace PaymentGateway.Infrastructure.Clients
{
    public class PaymentGatewayClient(IHttpClientFactory httpClientFactory, IOptions<PaymentGatewayOptions> options, ILogger<PaymentGatewayClient> logger) : IPaymentGatewayClient
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ILogger<PaymentGatewayClient> _logger = logger;
        private const string HttpClientName = "PaymentGateway";
        private readonly PaymentGatewayOptions _options = options.Value;

        public async Task<ApiResponse<PaymentGatewayResponse?>> ProcessPaymentAsync(PaymentGatewayRequest request, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient(HttpClientName);

            _logger.LogInformation("Sending payment request to gateway. Amount: {Amount} {Currency}", request.Amount, request.Currency);

            var fullUrl = $"{_options.BaseUrl.TrimEnd('/')}{_options.PaymentsPath}";

            var response = await httpClient.PostAsJsonAsync(fullUrl, request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<PaymentGatewayResponse>(cancellationToken: cancellationToken);
            return new ApiResponse<PaymentGatewayResponse?>
            {
                Success = true,
                Payload = result,
                StatusCode = response.StatusCode,
                Message = "Payment processed by gateway"
            };
        }
    }
}