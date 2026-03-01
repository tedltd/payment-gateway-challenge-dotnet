using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Application.Models
{
    public class PostPaymentResponse
    {
        public Guid Id { get; set; }
        public PaymentStatus Status { get; set; }
        public int CardNumberLastFour { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string Currency { get; set; } = string.Empty;
        public int Amount { get; set; }
    }
}
