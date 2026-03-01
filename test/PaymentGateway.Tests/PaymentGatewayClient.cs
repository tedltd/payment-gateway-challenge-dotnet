using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;
using Moq.Protected;

using PaymentGateway.Application.Models;
using PaymentGateway.Infrastructure.Clients;
using PaymentGateway.Infrastructure.Options;

namespace PaymentGateway.Tests;

[TestFixture]
public class PaymentGatewayClientTests
{
    private Mock<IHttpClientFactory> _mockHttpClientFactory;
    private Mock<IOptions<PaymentGatewayOptions>> _mockOptions;
    private Mock<IConfiguration> _mockConfiguration;
    private Mock<ILogger<PaymentGatewayClient>> _mockLogger;
    private Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private HttpClient _httpClient;
    private PaymentGatewayClient _client;
    private PaymentGatewayOptions _options;

    [SetUp]
    public void SetUp()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockOptions = new Mock<IOptions<PaymentGatewayOptions>>();

        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<PaymentGatewayClient>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:8080")
        };

        _mockHttpClientFactory
            .Setup(x => x.CreateClient("PaymentGateway"))
            .Returns(_httpClient);

        _mockConfiguration
             .Setup(x => x["PaymentGateway:BaseUrl"])
             .Returns("http://localhost:8080");

        _options = new PaymentGatewayOptions
        {
            BaseUrl = "http://localhost:8080",
            ApiKey = "",
            PaymentsPath = "/payments"
        };

        _mockOptions
            .Setup(x => x.Value)
            .Returns(_options);
        _mockConfiguration
            .Setup(x => x["PaymentsGatewayUrl"])
            .Returns("/payments");


        _client = new PaymentGatewayClient(
            _mockHttpClientFactory.Object,
            _mockOptions.Object,
            _mockLogger.Object);
    }

    [Test]
    public async Task ProcessPaymentAsync_ReturnsAuthorizedResponse_WhenPaymentIsApproved()
    {
        // Arrange
        var request = new PaymentGatewayRequest
        {
            CardNumber = "1111111111111111",
            ExpiryDate = "06/2026",
            Cvv = "123",
            Amount = 100,
            Currency = "GBP"
        };

        var expectedResponse = new PaymentGatewayResponse
        {
            Authorized = true,
            AuthorizationCode = "abc123-def456-ghi789"
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(expectedResponse)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains("/payments")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _client.ProcessPaymentAsync(request, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Payload, Is.Not.Null);
            Assert.That(result.Payload!.Authorized, Is.True);
            Assert.That(result.Payload.AuthorizationCode, Is.EqualTo("abc123-def456-ghi789"));
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Message, Is.EqualTo("Payment processed by gateway"));
        });
    }

    [Test]
    public async Task ProcessPaymentAsync_ReturnsDeclinedResponse_WhenPaymentIsDeclined()
    {
        // Arrange
        var request = new PaymentGatewayRequest
        {
            CardNumber = "1111111111111112",
            ExpiryDate = "06/2026",
            Cvv = "123",
            Amount = 100,
            Currency = "GBP"
        };

        var expectedResponse = new PaymentGatewayResponse
        {
            Authorized = false,
            AuthorizationCode = string.Empty
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(expectedResponse)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _client.ProcessPaymentAsync(request, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Payload, Is.Not.Null);
            Assert.That(result.Payload!.Authorized, Is.False);
            Assert.That(result.Payload.AuthorizationCode, Is.Empty);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });
    }

    

    

    [Test]
    public async Task ProcessPaymentAsync_BuildsCorrectUrl_FromConfiguration()
    {
        // Arrange
        var request = new PaymentGatewayRequest
        {
            CardNumber = "2222405343248877",
            ExpiryDate = "04/2025",
            Cvv = "123",
            Amount = 100,
            Currency = "GBP"
        };

        var expectedResponse = new PaymentGatewayResponse
        {
            Authorized = true,
            AuthorizationCode = "test-code"
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(expectedResponse)
        };

        HttpRequestMessage? capturedRequest = null;

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(responseMessage);

        // Act
        await _client.ProcessPaymentAsync(request, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(capturedRequest, Is.Not.Null);
            Assert.That(capturedRequest!.RequestUri!.ToString(), Does.Contain("http://localhost:8080/payments"));
            Assert.That(capturedRequest.Method, Is.EqualTo(HttpMethod.Post));
        });
    }

    [Test]
    public async Task ProcessPaymentAsync_HandlesCancellation_WhenCancellationRequested()
    {
        // Arrange
        var request = new PaymentGatewayRequest
        {
            CardNumber = "2222405343248877",
            ExpiryDate = "04/2025",
            Cvv = "123",
            Amount = 100,
            Currency = "GBP"
        };

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        // Act & Assert
        Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await _client.ProcessPaymentAsync(request, cancellationTokenSource.Token));
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
    }
}
