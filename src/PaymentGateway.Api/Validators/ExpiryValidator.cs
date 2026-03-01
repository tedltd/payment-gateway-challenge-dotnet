using FluentValidation;
using FluentValidation.Results;
using PaymentGateway.Domain.Request;

namespace PaymentApi.Validators
{
    public class ExpiryValidator : AbstractValidator<PaymentRequest>
    {
        public ExpiryValidator()
        {
            RuleFor(x => x.ExpiryMonth)
               .InclusiveBetween(1, 12).WithMessage("ExpiryMonthRange" ?? "{PropertyName} must be between {From} and {To}.")
               .WithErrorCode("EXPIRY_MONTH_INVALID");

            RuleFor(x => x.ExpiryYear)
                .NotEmpty().WithMessage("ExpiryYearRequired" ?? "{PropertyName} is required.")
                .Matches(@"^\d{4}$").WithMessage("ExpiryYearFormat" ?? "{PropertyName} must be a 4-digit year (YYYY).")
                .WithErrorCode("EXPIRY_YEAR_INVALID");


            RuleFor(x => x).Custom((req, ctx) =>
            {
                if (req.ExpiryMonth < 1 || req.ExpiryMonth > 12) return;
                if (string.IsNullOrWhiteSpace(req.ExpiryYear)) return;

                if (!int.TryParse(req.ExpiryYear, out var year))
                {
                    ctx.AddFailure(new ValidationFailure(nameof(req.ExpiryYear), "ExpiryDateInvalid" ?? "Expiry month/year is invalid.") { ErrorCode = "EXPIRY_DATE_INVALID" });
                    return;
                }

                try
                {
                    var now = DateTime.UtcNow;
                    var expiry = new DateTime(year, req.ExpiryMonth, 1);
                    var current = new DateTime(now.Year, now.Month, 1);

                    if (expiry < current)
                    {
                        var message = "ExpiryDateInPast" ?? "Expiry date must be the current month or later.";

                        var yearFailure = new ValidationFailure(nameof(req.ExpiryYear), message)
                        {
                            ErrorCode = "EXPIRY_DATE_PAST",
                            CustomState = new { attempted = $"{req.ExpiryMonth}/{req.ExpiryYear}" }
                        };
                        var monthFailure = new ValidationFailure(nameof(req.ExpiryMonth), message)
                        {
                            ErrorCode = "EXPIRY_DATE_PAST",
                            CustomState = new { attempted = $"{req.ExpiryMonth}/{req.ExpiryYear}" }
                        };

                        ctx.AddFailure(yearFailure);
                        ctx.AddFailure(monthFailure);
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    var message = "ExpiryDateInvalid" ?? "Expiry month/year is invalid.";
                    ctx.AddFailure(new ValidationFailure(nameof(req.ExpiryMonth), message) { ErrorCode = "EXPIRY_DATE_INVALID" });
                }
            });
        }
    }
}
