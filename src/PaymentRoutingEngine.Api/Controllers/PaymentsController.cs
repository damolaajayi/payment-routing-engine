using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PaymentRoutingEngine.Api.Contracts.Requests;
using PaymentRoutingEngine.Api.Contracts.Responses;
using PaymentRoutingEngine.Application.Abstractions.Messaging;
using PaymentRoutingEngine.Application.Payments.CreatePayment;
using PaymentRoutingEngine.Application.Payments.GetPaymentById;
using PaymentRoutingEngine.Application.Payments.ProcessPayment;
using PaymentRoutingEngine.Infrastructure.Messaging;

namespace PaymentRoutingEngine.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public sealed class PaymentsController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;
        private readonly IPaymentProcessingQueue _paymentProcessingQueue;

        public PaymentsController(IDispatcher dispatcher, IPaymentProcessingQueue paymentProcessingQueue)
        {
            _dispatcher = dispatcher;
            _paymentProcessingQueue = paymentProcessingQueue;
        }

        [HttpPost]
        [EnableRateLimiting("CreatePaymentPolicy")]
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
        [EnableRateLimiting("ProcessPaymentPolicy")]
        public async Task<IActionResult> ProcessPayment(Guid transactionId, CancellationToken cancellationToken)
        {
            await _paymentProcessingQueue.EnqueueAsync(transactionId, cancellationToken);

            return Accepted(new
            {
                transactionId,
                status = "Queued"
            });
            
        }

    }
}
