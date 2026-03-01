
namespace PaymentGateway.Infrastructure.Options
{
    public class CacheOptions
    {
        public const string SectionName = "CacheOptions";
        public bool Enabled { get; set; } = true;
        public int CacheMinutes { get; set; } = 30;

    }
}
