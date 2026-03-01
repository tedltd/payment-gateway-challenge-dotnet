using FluentValidation;
using PaymentGateway.Domain.Request;

namespace PaymentApi.Validators
{
    public class CardNumberValidator : AbstractValidator<PaymentRequest>
    {
        public CardNumberValidator()
        {
            RuleFor(x => x.CardNumber)
                .NotEmpty().WithMessage("CardNumber is required.")
                .Matches(@"^\d{14,19}$").WithMessage("{PropertyName} must be 14 to 19 numeric characters.")
                .WithErrorCode("CARD_NUMBER_INVALID");

            //RuleFor(x => x.CardNumber)
            //    .NotEmpty().WithMessage("CardNumberRequired" ?? "{PropertyName} is required.")
            //    .Length(14, 19).WithMessage("{PropertyName} must be between {MinLength} and {MaxLength} characters.")
            //    .Must(card => !string.IsNullOrEmpty(card) && card.All(char.IsDigit))
            //        .WithMessage("{PropertyName} must contain only numeric characters.")
            //    .WithErrorCode("CARD_NUMBER_INVALID");
        }
    }
}
