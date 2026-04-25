using PaymentRoutingEngine.Application.Payments.GetPaymentById;

namespace PaymentRoutingEngine.Api.Contracts.Responses
{
    public sealed class GetPaymentResponse
    {
        public Guid TransactionId { get; init; }
        public string Reference { get; init; } = default!;
        public string? ClientReference { get; init; }
        public string? CustomerId { get; init; }
        public long AmountMinor { get; init; }
        public string Currency { get; init; } = default!;
        public string Status { get; init; } = default!;
        public string? FailureReason { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? CompletedAtUtc { get; init; }


        public GetPaymentResponse(GetPaymentByIdResult result)
        {
            TransactionId = result.TransactionId;
            Reference = result.Reference;
            ClientReference = result.ClientReference;
            CustomerId = result.CustomerId;
            AmountMinor = result.AmountMinor;
            Currency = result.Currency;
            Status = result.Status.ToString();
            FailureReason = result.FailureReason;
            CreatedAtUtc = result.CreatedAtUtc;
            CompletedAtUtc = result.CompletedAtUtc;
        }

        public static GetPaymentResponse From(GetPaymentByIdResult result) => new(result);
    }
}
