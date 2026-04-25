using PaymentRoutingEngine.Application.Payments.CreatePayment;

namespace PaymentRoutingEngine.Api.Contracts.Responses
{
    public sealed class CreatePaymentResponse
    {
        public Guid TransactionId { get; init; }
        public string Reference { get; init; } = default!;
        public string? IdempotencyKey { get; init; }
        public long AmountMinor { get; init; }
        public string Currency { get; init; } = default!;
        public string Status { get; init; } = default!;
        public DateTime CreatedAtUtc { get; init; }


        public CreatePaymentResponse(CreatePaymentResult result)
        {
            TransactionId = result.TransactionId;
            Reference = result.Reference;
            IdempotencyKey = result.IdempotencyKey;
            AmountMinor = result.AmountMinor;
            Currency = result.Currency;
            Status = result.Status.ToString();
            CreatedAtUtc = result.CreatedAtUtc;
        }

        public static CreatePaymentResponse From(CreatePaymentResult result) => new(result);
    }
}
