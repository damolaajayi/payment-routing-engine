using PaymentRoutingEngine.Application.Payments.CreatePayment;

namespace PaymentRoutingEngine.Api.Contracts.Requests
{
    public sealed class CreatePaymentRequest
    {
        public string? IdempotencyKey { get; init; }
        public string? ClientReference { get; init; }
        public string? CustomerId { get; init; }
        public long AmountMinor { get; init; }
        public required string Currency { get; init; }
        public string? Description { get; init; }


        public CreatePaymentCommand ToCommand()
        {
            return new CreatePaymentCommand(
                IdempotencyKey: IdempotencyKey,
                ClientReference: ClientReference,
                CustomerId: CustomerId,
                AmountMinor: AmountMinor,
                Currency: Currency,
                Description: Description);
        }
    }
}
