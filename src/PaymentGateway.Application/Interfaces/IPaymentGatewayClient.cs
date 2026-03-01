using PaymentGateway.Application.Models;
using PaymentGateway.Domain.Response;

namespace PaymentGateway.Application.Interfaces
{
    public interface IPaymentGatewayClient
    {
        Task<ApiResponse<PaymentGatewayResponse?>> ProcessPaymentAsync(PaymentGatewayRequest request, CancellationToken cancellationToken);
    }
}
