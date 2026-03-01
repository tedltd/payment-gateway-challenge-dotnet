using System.Text.Json.Serialization;

namespace PaymentGateway.Application.Models
{
    public class PaymentGatewayResponse
    {
        [JsonPropertyName("authorized")]
        public bool Authorized { get; set; }

        [JsonPropertyName("authorization_code")]
        public string AuthorizationCode { get; set; } = string.Empty;

    }
}
