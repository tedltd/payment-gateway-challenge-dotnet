namespace PaymentApi.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using global::PaymentApi.Validators;

    using Moq;
    using NUnit.Framework;

    using PaymentGateway.Application.Interfaces;
    using PaymentGateway.Domain.Request;

    namespace PaymentApi.Tests.Validators
    {
        [TestFixture]
        public class CardNumberValidatorTests
        {
            private CardNumberValidator _validator = null!;

            [SetUp]
            public void SetUp() => _validator = new CardNumberValidator();

            [Test]
            public async Task ValidCardNumber_16Digits_Passes()
            {
                var request = new PaymentRequest { CardNumber = "1234567890123456" }; 
                var result = await _validator.ValidateAsync(request).ConfigureAwait(false);
                Assert.That(result.IsValid, Is.True);
            }

            [Test]
            public async Task EmptyCardNumber_Fails()
            {
                var request = new PaymentRequest { CardNumber = string.Empty };
                var result = await _validator.ValidateAsync(request).ConfigureAwait(false);
                Assert.That(result.IsValid, Is.False);
            }

            [Test]
            public async Task NonDigitCardNumber_Fails()
            {
                var request = new PaymentRequest { CardNumber = "1111-1111-xxxx-xxxx" };
                var result = await _validator.ValidateAsync(request).ConfigureAwait(false);
                Assert.That(result.IsValid, Is.False);
            }

            [Test]
            public async Task ShortCardNumber_Less_Than_14Digits_Fails()
            {
                var request = new PaymentRequest { CardNumber = "1234567890123" }; // 13 digits - below 14
                var result = await _validator.ValidateAsync(request).ConfigureAwait(false);
                Assert.That(result.IsValid, Is.False);
            }
        }

        [TestFixture]
        public class CvvValidatorTests
        {
            private CvvValidator _validator = null!;

            [SetUp]
            public void SetUp() => _validator = new CvvValidator();

            [TestCase("123")]
            [TestCase("1234")]
            public async Task ValidCvv_Passes(string cvv)
            {
                var request = new PaymentRequest { Cvv = cvv };
                var result = await _validator.ValidateAsync(request).ConfigureAwait(false);
                Assert.That(result.IsValid, Is.True);
            }

            [Test]
            public async Task EmptyCvv_Fails()
            {
                var request = new PaymentRequest { Cvv = string.Empty };
                var result = await _validator.ValidateAsync(request).ConfigureAwait(false);
                Assert.That(result.IsValid, Is.False);
            }

            [TestCase("12a")]
            [TestCase("12")]
            [TestCase("12345")]
            public async Task InvalidCvv_Fails(string cvv)
            {
                var request = new PaymentRequest { Cvv = cvv };
                var result = await _validator.ValidateAsync(request).ConfigureAwait(false);
                Assert.That(result.IsValid, Is.False);
            }
        }

        [TestFixture]
        public class CurrencyValidatorTests
        {
            private Mock<ICurrencyRepository> _currencyRepoMock = null!;

            [SetUp]
            public void SetUp()
            {
                _currencyRepoMock = new Mock<ICurrencyRepository>();
                _currencyRepoMock
                    .Setup(r => r.GetValidIsoCurrenciesAsync())
                    .ReturnsAsync(["USD", "EUR", "GBP"]);
            }

            [Test]
            public async Task ValidCurrency_Passes()
            {
                var validator = new CurrencyValidator(_currencyRepoMock.Object);
                var request = new PaymentRequest { Currency = "USD" };
                var result = await validator.ValidateAsync(request).ConfigureAwait(false);
                Assert.IsTrue(result.IsValid);
            }

            [Test]
            public async Task InvalidCurrency_Fails()
            {
                var validator = new CurrencyValidator(_currencyRepoMock.Object);
                var request = new PaymentRequest { Currency = "ABC" };
                var result = await validator.ValidateAsync(request).ConfigureAwait(false);
                Assert.That(result.IsValid, Is.False);
            }

            [Test]
            public async Task EmptyCurrency_Fails()
            {
                var validator = new CurrencyValidator(_currencyRepoMock.Object);
                var request = new PaymentRequest { Currency = string.Empty };
                var result = await validator.ValidateAsync(request).ConfigureAwait(false);
                Assert.That(result.IsValid, Is.False);
            }
        }
    }
}
