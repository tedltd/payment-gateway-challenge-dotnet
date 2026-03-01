using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Domain.Request
{
    public class PaymentRequest
    {
        /// <summary>Card number (digits only).</summary>
        [Required]
        [StringLength(19, MinimumLength = 14)]
        [RegularExpression(@"^\d+$", ErrorMessage = "CardNumber must contain only digits.")]
        public string CardNumber { get; set; } = null!;

        /// <summary>Expiry month (1-12).</summary>
        [Required]
        [Range(1, 12)]
        public int ExpiryMonth { get; set; }

        /// <summary>Expiry year (YYYY).</summary>
        [Required]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "ExpiryYear must be a 4-digit year (YYYY).")]
        public string ExpiryYear { get; set; } = null!;

        /// <summary>CVV: 3 or 4 numeric characters.</summary>
        [Required]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "Cvv must be 3 or 4 digits.")]
        public string Cvv { get; set; } = null!;

        /// <summary>Amount in minor units (integer, >= 0).</summary>
        [Range(0, int.MaxValue)]
        public int Amount { get; set; }

        /// <summary>Currency (ISO 4217, allowed: USD, EUR, GBP).</summary>
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; } = null!;
    }
}
