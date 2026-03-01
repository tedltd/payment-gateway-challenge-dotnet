using AutoMapper;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Models;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Request;
using PaymentGateway.Domain.Response;
using System.Diagnostics;

namespace PaymentGateway.Api.Services
{
    public class PaymentService(
        ICurrencyRepository currencyRepository,
        IPaymentRepository paymentRepository,
        IPaymentGatewayClient gatewayClient,
        ILogger<PaymentService> logger,
        PaymentMetrics metrics,
        IMapper mapper) : IPaymentService
    {
        private readonly ICurrencyRepository _currencyRepository = currencyRepository;
        private readonly IPaymentRepository _paymentRepository = paymentRepository;
        private readonly IPaymentGatewayClient _gatewayClient = gatewayClient;
        private readonly ILogger<PaymentService> _logger = logger;
        private readonly PaymentMetrics _metrics = metrics;
        private readonly IMapper _mapper = mapper;

        public async Task<ApiResponse<PostPaymentResponse>> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation("Processing payment request for card ending in {CardNumberLast4}", request?.CardNumber?[^4..]);
            if (request == null)
            {
                _logger.LogWarning("Payment request is null");
                _metrics.RecordPaymentFailed(null, "NullRequest");
                return new ApiResponse<PostPaymentResponse>
                {
                    Success = false,
                    Message = "Request cannot be null.",
                    ErrorMessage = "Invalid request."
                };
            }
            var gatewayRequest = _mapper.Map<PaymentGatewayRequest>(request);
            var gatewayResponse = await _gatewayClient.ProcessPaymentAsync(gatewayRequest, cancellationToken);
            var payload = gatewayResponse.Payload;

            // TODO auto mapper
            var paymentResponse = new PostPaymentResponse
            {
                Id = string.IsNullOrEmpty(payload!.AuthorizationCode) ? Guid.NewGuid() : Guid.Parse(payload.AuthorizationCode),
                CardNumberLastFour = int.Parse(request.CardNumber[^4..]),
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = int.Parse(request.ExpiryYear),
                Amount = request.Amount,
                Status = payload.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined
            };
            stopwatch.Stop();

            _paymentRepository.Add(paymentResponse);

            _logger.LogInformation("Payment processed successfully. PaymentId: {PaymentId}, Status: {Status}", paymentResponse.Id, paymentResponse.Status);
            _metrics.RecordPaymentProcessed(request.Currency, request.Amount, paymentResponse.Status);
            _metrics.RecordPaymentDuration(stopwatch.Elapsed.TotalMilliseconds, request.Currency, paymentResponse.Status);

            return new ApiResponse<PostPaymentResponse>(paymentResponse) { StatusCode = gatewayResponse.StatusCode };

        }

        public Task<PostPaymentResponse> GetPayment(Guid id)
        {
            var payment = _paymentRepository.Get(id);
            if (payment == null)
            {
                _logger.LogWarning("Payment with ID {PaymentId} not found", id);
                return Task.FromResult<PostPaymentResponse>(null);
            }
            var response = new PostPaymentResponse
            {
                Id = payment.Id,
                Status = payment.Status,
                CardNumberLastFour = payment.CardNumberLastFour,
                ExpiryMonth = payment.ExpiryMonth,
                ExpiryYear = payment.ExpiryYear,
                Currency = payment.Currency,
                Amount = payment.Amount
            };
            return Task.FromResult(response);
        }

        public async Task<List<string>> GetValidIsoCurrenciesAsync()
        {
            _logger.LogDebug("Retrieving valid ISO currencies");
            return await _currencyRepository.GetValidIsoCurrenciesAsync();
        }
    }
}
