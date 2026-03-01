using FluentValidation;
using PaymentGateway.Domain.Request;
using PaymentGateway.Application.Interfaces;


namespace PaymentApi.Validators
{
    public class CurrencyValidator : AbstractValidator<PaymentRequest>
    {
        public CurrencyValidator(ICurrencyRepository currencyRepository)
        {
            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("CurrencyRequired" ?? "{PropertyName} is required.")
                .Length(3).WithMessage("CurrencyLength" ?? "{PropertyName} must be a 3-letter ISO code.")
                .MustAsync(async (currency, cancellation) =>
                {
                    if (string.IsNullOrWhiteSpace(currency))
                        return false;

                    var allowedCurrencies = await currencyRepository.GetValidIsoCurrenciesAsync();
                    return allowedCurrencies.Contains(currency.Trim().ToUpperInvariant());
                })
                .WithMessage("CurrencyUnsupported" ?? "{PropertyName} must be one of: USD, EUR, GBP.")
                .WithErrorCode("CURRENCY_INVALID");
        }
    }
}
