using System.Net;

using Microsoft.AspNetCore.Mvc;

using Moq;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Models;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Request;
using PaymentGateway.Domain.Response;

namespace PaymentGateway.Tests;

[TestFixture]
public class PaymentsControllerTests
{
    private Mock<IPaymentService> _mockPaymentService;
    private PaymentsController _controller;
    private Random _random;

    [SetUp]
    public void SetUp()
    {
        _random = new Random();
        _mockPaymentService = new Mock<IPaymentService>();
        _controller = new PaymentsController(_mockPaymentService.Object);
    }

    [Test]
    public async Task GetPayment_ReturnsOkResult_WhenPaymentExists()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var expectedPayment = new PostPaymentResponse
        {
            Id = paymentId,
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = _random.Next(1111, 9999),
            Currency = "GBP",
            Status = PaymentStatus.Authorized
        };

        _mockPaymentService
            .Setup(x => x.GetPayment(paymentId))
            .ReturnsAsync(expectedPayment);

        // Act
        var result = await _controller.GetPayment(paymentId);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));

        var paymentResponse = okResult.Value as PostPaymentResponse;

        Assert.Multiple(() =>
        {
            Assert.That(paymentResponse, Is.Not.Null);
            Assert.That(paymentResponse!.Id, Is.EqualTo(paymentId));
            Assert.That(paymentResponse.Currency, Is.EqualTo("GBP"));
        });

        _mockPaymentService.Verify(x => x.GetPayment(paymentId), Times.Once);
    }

    [Test]
    public async Task GetPayment_ReturnsNotFound_WhenPaymentDoesNotExist()
    {
        // Arrange
        var paymentId = Guid.NewGuid();

        _mockPaymentService
            .Setup(x => x.GetPayment(paymentId))
            .ReturnsAsync((PostPaymentResponse?)null);

        // Act
        var result = await _controller.GetPayment(paymentId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));

        _mockPaymentService.Verify(x => x.GetPayment(paymentId), Times.Once);
    }

    [Test]
    public async Task GetPayment_ReturnsBadRequest_WhenGuidIsEmpty()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var result = await _controller.GetPayment(emptyGuid);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.StatusCode, Is.EqualTo(400));

        _mockPaymentService.Verify(x => x.GetPayment(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task Post_ReturnsOkResult_WhenPaymentIsAuthorized()
    {
        // Arrange
        var paymentRequest = new PaymentRequest
        {
            CardNumber = "2222405343248877",
            ExpiryMonth = 4,
            ExpiryYear = "2025",
            Currency = "GBP",
            Amount = 100,
            Cvv = "123"
        };

        var expectedResponse = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = 8877,
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100
        };

        var apiResponse = new ApiResponse<PostPaymentResponse>
        {
            Success = true,
            Payload = expectedResponse,
            StatusCode = HttpStatusCode.OK,
            Message = "Payment processed successfully"
        };

        _mockPaymentService
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.Post(paymentRequest, CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);

        _mockPaymentService.Verify(
            x => x.ProcessPaymentAsync(It.Is<PaymentRequest>(r =>
                r.CardNumber == paymentRequest.CardNumber &&
                r.Amount == paymentRequest.Amount &&
                r.Currency == paymentRequest.Currency),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Post_ReturnsAppropriateResult_WhenPaymentIsDeclined()
    {
        // Arrange
        var paymentRequest = new PaymentRequest
        {
            CardNumber = "2222405343248888",
            ExpiryMonth = 4,
            ExpiryYear = "2025",
            Currency = "GBP",
            Amount = 100,
            Cvv = "123"
        };

        var expectedResponse = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Declined,
            CardNumberLastFour = 8888,
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Amount = 100
        };

        var apiResponse = new ApiResponse<PostPaymentResponse>
        {
            Success = false,
            Payload = expectedResponse,
            StatusCode = HttpStatusCode.OK,
            Message = "Payment declined"
        };

        _mockPaymentService
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.Post(paymentRequest, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);

        _mockPaymentService.Verify(
            x => x.ProcessPaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Post_HandlesServiceErrors_Gracefully()
    {
        // Arrange
        var paymentRequest = new PaymentRequest
        {
            CardNumber = "2222405343248877",
            ExpiryMonth = 4,
            ExpiryYear = "2025",
            Currency = "GBP",
            Amount = 100,
            Cvv = "123"
        };

        var apiResponse = new ApiResponse<PostPaymentResponse>
        {
            Success = false,
            Payload = null,
            StatusCode = HttpStatusCode.ServiceUnavailable,
            Message = "Service temporarily unavailable"
        };

        _mockPaymentService
            .Setup(x => x.ProcessPaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.Post(paymentRequest, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);

        _mockPaymentService.Verify(
            x => x.ProcessPaymentAsync(It.IsAny<PaymentRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}