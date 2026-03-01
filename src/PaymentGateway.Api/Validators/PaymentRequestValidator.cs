using FluentValidation;
using PaymentGateway.Domain.Request;

namespace PaymentApi.Validators
{
    public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
    {
        public PaymentRequestValidator(
            CardNumberValidator cardNumberValidator,ExpiryValidator expiryValidator,
            CvvValidator cvvValidator, CurrencyValidator currencyValidator, AmountValidator amountValidator)
        {
            Include(cardNumberValidator);
            Include(expiryValidator);
            Include(cvvValidator);
            Include(currencyValidator);
            Include(amountValidator);
        }
    }

}
