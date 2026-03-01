using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Models;
using PaymentGateway.Infrastructure.Options;

namespace PaymentGateway.Infrastructure.Repositories
{
    
    public class PaymentsRepository(IMemoryCache cache, IOptions<CacheOptions> options) : IPaymentRepository
    {
        private readonly IMemoryCache _cache = cache;
        private readonly CacheOptions _options = options.Value;
        private readonly List<PostPaymentResponse> _payments = [];
        private const string PaymentsListCacheKey = "allPayments";

        public void Add(PostPaymentResponse payment)
        {
            _payments.Add(payment);
            _cache.Set(PaymentsListCacheKey, _payments, TimeSpan.FromMinutes(_options.CacheMinutes));
        }

        public PostPaymentResponse? Get(Guid id)
        {
            return _cache.TryGetValue(PaymentsListCacheKey, out List<PostPaymentResponse>? cachedPayments)
                ? (cachedPayments?.SingleOrDefault(x=> x.Id == id))
                : null;
        }
    }
}
