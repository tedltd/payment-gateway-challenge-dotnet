
namespace PaymentGateway.Application.Interfaces
{
    public interface ICurrencyRepository
    {
        Task<List<string>> GetValidIsoCurrenciesAsync();
    }
}
