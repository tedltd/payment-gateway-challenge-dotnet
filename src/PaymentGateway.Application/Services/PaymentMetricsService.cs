using PaymentGateway.Domain.Enums;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace PaymentGateway.Application.Services
{
    public class PaymentMetricsService
    {
        private readonly Counter<long> _paymentProcessedCounter;
        private readonly Counter<long> _paymentFailedCounter;
        private readonly Histogram<double> _paymentProcessingDuration;
        private readonly Counter<long> _validationFailureCounter;
        private readonly Counter<long> _authorizationFailureCounter;

        public const string MeterName = "PaymentApi";

        public PaymentMetricsService(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create(MeterName);

            _paymentProcessedCounter = meter.CreateCounter<long>(
                "payment.processed",
                description: "Number of successfully processed payments");

            _paymentFailedCounter = meter.CreateCounter<long>(
                "payment.failed",
                description: "Number of failed payment attempts");

            _paymentProcessingDuration = meter.CreateHistogram<double>(
                "payment.processing.duration",
                unit: "ms",
                description: "Duration of payment processing in milliseconds");

            _validationFailureCounter = meter.CreateCounter<long>(
                "payment.validation.failed",
                description: "Number of payment validation failures");

            _authorizationFailureCounter = meter.CreateCounter<long>(
                "payment.authorization.failed",
                description: "Number of authorization failures");
        }

        public void RecordPaymentProcessed(string currency, decimal amount, PaymentStatus status)
        {
            var tags = new TagList
            {
                { "currency", currency },
                { "status", status.ToString() }
            };
            _paymentProcessedCounter.Add(1, tags);
        }

        public void RecordPaymentFailed(string? currency, string reason)
        {
            var tags = new TagList
            {
                { "currency", currency ?? "unknown" },
                { "reason", reason }
            };
            _paymentFailedCounter.Add(1, tags);
        }

        public void RecordPaymentDuration(double durationMs, string currency, PaymentStatus status)
        {
            var tags = new TagList
            {
                { "currency", currency },
                { "status", status.ToString() }
            };
            _paymentProcessingDuration.Record(durationMs, tags);
        }

        public void RecordValidationFailure(string fieldName)
        {
            var tags = new TagList { { "field", fieldName } };
            _validationFailureCounter.Add(1, tags);
        }

        public void RecordAuthorizationFailure(string reason)
        {
            var tags = new TagList { { "reason", reason } };
            _authorizationFailureCounter.Add(1, tags);
        }
    }
}