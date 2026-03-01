using PaymentGateway.Application.Models;

namespace PaymentGateway.Application.Interfaces
{
    public interface IPaymentRepository
    {
        void Add(PostPaymentResponse payment);
        PostPaymentResponse? Get(Guid id);
    }
}