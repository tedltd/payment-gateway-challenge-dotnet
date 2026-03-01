using FluentValidation;
using PaymentGateway.Domain.Request;

namespace PaymentApi.Validators
{
    public class CvvValidator : AbstractValidator<PaymentRequest>
    {
        public CvvValidator()
        {
            RuleFor(x => x.Cvv)
                .NotEmpty().WithMessage("CvvRequired" ?? "{PropertyName} is required.")
                .Matches(@"^\d{3,4}$").WithMessage("CvvDigits" ?? "{PropertyName} must be 3 or 4 digits.")
                .WithErrorCode("CVV_INVALID");
        }
    }
}
