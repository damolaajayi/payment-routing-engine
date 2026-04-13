using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Domain.Entities
{
    public sealed class TransactionStatusHistoryEntry
    {
        private TransactionStatusHistoryEntry()
        {
        }

        private TransactionStatusHistoryEntry(
            Guid id,
            Guid paymentTransactionId,
            TransactionStatus? previousStatus,
            TransactionStatus newStatus,
            string? reason,
            string source,
            DateTime changedAtUtc,
            string? metadata)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("Source is required.", nameof(source));

            Id = id;
            PaymentTransactionId = paymentTransactionId;
            PreviousStatus = previousStatus;
            NewStatus = newStatus;
            Reason = reason;
            Source = source.Trim();
            ChangedAtUtc = changedAtUtc;
            Metadata = metadata;
        }

        public Guid Id { get; private set; }
        public Guid PaymentTransactionId { get; private set; }
        public TransactionStatus? PreviousStatus { get; private set; }
        public TransactionStatus NewStatus { get; private set; }
        public string? Reason { get; private set; }
        public string Source { get; private set; } = default!;
        public DateTime ChangedAtUtc { get; private set; }
        public string? Metadata { get; private set; }

        public static TransactionStatusHistoryEntry Create(
            Guid paymentTransactionId,
            TransactionStatus? previousStatus,
            TransactionStatus newStatus,
            string? reason,
            string source,
            DateTime changedAtUtc,
            string? metadata = null)
        {
            return new TransactionStatusHistoryEntry(
                id: Guid.NewGuid(),
                paymentTransactionId: paymentTransactionId,
                previousStatus: previousStatus,
                newStatus: newStatus,
                reason: reason,
                source: source,
                changedAtUtc: changedAtUtc,
                metadata: metadata);
        }
    }
}
