using PaymentGateway.Application.Interfaces;

namespace PaymentGateway.Infrastructure.Repositories
{

    public class CurrencyRepository : ICurrencyRepository
    {
        public async Task<List<string>> GetValidIsoCurrenciesAsync()
        {
           return await Task.FromResult(new List<string> { "USD", "EUR", "GBP" });
        }
    }
}
