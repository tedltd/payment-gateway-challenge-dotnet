using FluentValidation;
using PaymentGateway.Domain.Request;

namespace PaymentApi.Validators
{
    public class AmountValidator : AbstractValidator<PaymentRequest>
    {
        public AmountValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThanOrEqualTo(0).WithMessage("AmountInvalid" ?? "{PropertyName} must be an integer >= 0.")
                .WithErrorCode("AMOUNT_INVALID");

        }
    }
}
