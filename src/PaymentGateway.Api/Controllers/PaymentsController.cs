using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Attributes;
using PaymentGateway.Api.Extensions;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Domain.Request;

namespace PaymentGateway.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RequireAuthorization]
    public class PaymentsController(IPaymentService paymentService) : ControllerBase
    {
        private readonly IPaymentService _paymentService = paymentService;

        // Allow anonymous access because i don't know if you need to run this or not and i dont really want to add authorisation for this solution
        // should also add versioning.
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] PaymentRequest request, CancellationToken cancellationToken)
        {
            // should make controllers thin
            var response = await _paymentService.ProcessPaymentAsync(request, cancellationToken);
            return response.ToActionResult();
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPayment([FromRoute] Guid id)
        {
            // could do the same as i have for post
            if (id == Guid.Empty)
            {
                return BadRequest(new { errors = new[] { "Invalid payment ID." } });
            }

            var payment = await _paymentService.GetPayment(id);
            if (payment == null)
            {
                return NotFound(new { errors = new[] { "Payment not found." } });
            }
            return Ok(payment);

        }
    }
}
