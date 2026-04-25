using PaymentRoutingEngine.Application.Abstractions.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Payments.HandleWebhook
{
    public sealed record HandlePaystackWebhookCommand(
    string Payload,
    string? Signature) : ICommand<Unit>;
}
