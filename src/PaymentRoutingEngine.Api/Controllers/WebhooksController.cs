using Microsoft.AspNetCore.Mvc;
using PaymentRoutingEngine.Application.Abstractions.Messaging;
using PaymentRoutingEngine.Application.Payments.HandleWebhook;

namespace PaymentRoutingEngine.Api.Controllers
{
    [ApiController]
    [Route("api/webhooks/paystack")]
    public sealed class WebhooksController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;

        public WebhooksController(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook(CancellationToken cancellationToken)
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync(cancellationToken);

            var signature = Request.Headers["x-paystack-signature"].FirstOrDefault();

            var command = new HandlePaystackWebhookCommand(body, signature);

            await _dispatcher.Send(command, cancellationToken);

            return Ok(); // IMPORTANT: always 200 if processed
        }
    }
}
