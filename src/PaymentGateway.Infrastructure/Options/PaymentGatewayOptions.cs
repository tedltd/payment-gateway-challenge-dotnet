namespace PaymentGateway.Infrastructure.Options
{
    public class PaymentGatewayOptions
    {
        public const string SectionName = "PaymentGateway";

        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string PaymentsPath { get; set; } = "/payments";
    }
}
