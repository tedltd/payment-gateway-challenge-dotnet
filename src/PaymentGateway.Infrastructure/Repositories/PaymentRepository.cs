using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Models;
using PaymentGateway.Infrastructure.Options;

namespace PaymentGateway.Infrastructure.Repositories
{
    
    public class PaymentRepository(IMemoryCache cache, IOptions<CacheOptions> options) : IPaymentRepository
    {
        private readonly IMemoryCache _cache = cache;
        private readonly CacheOptions _options = options.Value;
        private const string PaymentsListCacheKey = "allPayments";

        public void Add(PostPaymentResponse payment)
        {
            var payments = GetAllPayments();
            payments.Add(payment);
            _cache.Set(PaymentsListCacheKey, payments, TimeSpan.FromMinutes(_options.CacheMinutes));
        }

        public PostPaymentResponse? Get(Guid id)
        {
            var payments = GetAllPayments();
            return payments.SingleOrDefault(x => x.Id == id);
        }
        private List<PostPaymentResponse> GetAllPayments()
        {
            if (!_cache.TryGetValue(PaymentsListCacheKey, out List<PostPaymentResponse>? cachedPayments))
            {
                cachedPayments = [];
                _cache.Set(PaymentsListCacheKey, cachedPayments, TimeSpan.FromMinutes(_options.CacheMinutes));
            }
            return cachedPayments ?? [];
        }
    }
}
