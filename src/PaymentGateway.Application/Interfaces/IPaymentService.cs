using PaymentGateway.Application.Models;
using PaymentGateway.Domain.Request;
using PaymentGateway.Domain.Response;

namespace PaymentGateway.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<ApiResponse<PostPaymentResponse>> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken);
        Task<List<string>> GetValidIsoCurrenciesAsync();
        Task<PostPaymentResponse> GetPayment(Guid id);
    }
}