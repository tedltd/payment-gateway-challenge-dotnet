using System.Text.Json.Serialization;

namespace PaymentGateway.Application.Models
{ 
    public class PaymentGatewayRequest
    {
        [JsonPropertyName("card_number")]
        public string CardNumber { get; set; } = null!;

        [JsonPropertyName("expiry_date")]
        public string ExpiryDate { get; set; } = null!;

        [JsonPropertyName("cvv")]
        public string Cvv { get; set; } = null!;

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = null!;
    }
}
