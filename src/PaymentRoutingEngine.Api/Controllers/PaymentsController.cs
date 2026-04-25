using Microsoft.AspNetCore.Mvc;
using PaymentRoutingEngine.Api.Contracts.Requests;
using PaymentRoutingEngine.Api.Contracts.Responses;
using PaymentRoutingEngine.Application.Abstractions.Messaging;
using PaymentRoutingEngine.Application.Payments.CreatePayment;
using PaymentRoutingEngine.Application.Payments.GetPaymentById;
using PaymentRoutingEngine.Application.Payments.ProcessPayment;

namespace PaymentRoutingEngine.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public sealed class PaymentsController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;

        public PaymentsController(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreatePaymentResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request, CancellationToken cancellationToken)
        {
            var result = await _dispatcher.Send(request.ToCommand(), cancellationToken);
            return CreatedAtAction(nameof(GetPayment), new { transactionId = result.TransactionId }, CreatePaymentResponse.From(result));
        }


        [HttpGet("{transactionId:guid}")]
        [ProducesResponseType(typeof(GetPaymentResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPayment(Guid transactionId, CancellationToken cancellationToken)
        {
            var result = await _dispatcher.Send(new GetPaymentByIdQuery(transactionId), cancellationToken);
            return Ok(GetPaymentResponse.From(result));
        }

        [HttpPost("{transactionId:guid}/process")]
        public async Task<IActionResult> ProcessPayment(Guid transactionId, CancellationToken cancellationToken)
        {
            var result = await _dispatcher.Send(new ProcessPaymentCommand(transactionId), cancellationToken);
            return Ok(new
            {
                result.TransactionId,
                Status = result.Status.ToString(),
                result.FailureReason
            });
        }

    }
}
