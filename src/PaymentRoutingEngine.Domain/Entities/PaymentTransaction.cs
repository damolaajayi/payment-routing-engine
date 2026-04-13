using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Domain.Entities
{
    public sealed class PaymentTransaction
    {
        private readonly List<PaymentAttempt> _attempts = [];
        private readonly List<TransactionStatusHistoryEntry> _statusHistory = [];

        private PaymentTransaction()
        {
        }

        private PaymentTransaction(
            Guid id,
            string reference,
            string? idempotencyKey,
            string? clientReference,
            string? customerId,
            long amountMinor,
            string currency,
            string? description,
            DateTime createdAtUtc)
        {
            if (string.IsNullOrWhiteSpace(reference))
                throw new ArgumentException("Reference is required.", nameof(reference));

            if (amountMinor <= 0)
                throw new ArgumentOutOfRangeException(nameof(amountMinor), "Amount must be greater than zero.");

            if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
                throw new ArgumentException("Currency must be a valid 3-letter ISO code.", nameof(currency));

            Id = id;
            Reference = reference.Trim();
            IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim();
            ClientReference = string.IsNullOrWhiteSpace(clientReference) ? null : clientReference.Trim();
            CustomerId = string.IsNullOrWhiteSpace(customerId) ? null : customerId.Trim();
            AmountMinor = amountMinor;
            Currency = currency.Trim().ToUpperInvariant();
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            Status = TransactionStatus.Pending;
            CreatedAtUtc = createdAtUtc;
            UpdatedAtUtc = createdAtUtc;
            Version = 1;

            AddStatusHistory(null, TransactionStatus.Pending, "Transaction created.", "system", createdAtUtc);
        }

        public Guid Id { get; private set; }
        public string Reference { get; private set; } = default!;
        public string? IdempotencyKey { get; private set; }
        public string? ClientReference { get; private set; }
        public string? CustomerId { get; private set; }
        public long AmountMinor { get; private set; }
        public string Currency { get; private set; } = default!;
        public TransactionStatus Status { get; private set; }
        public PaymentProvider? SelectedProvider { get; private set; }
        public RoutingDecisionType? RoutingDecisionType { get; private set; }
        public FailureCategory? FailureCategory { get; private set; }
        public string? FailureReason { get; private set; }
        public string? Description { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime UpdatedAtUtc { get; private set; }
        public DateTime? ProcessedAtUtc { get; private set; }
        public DateTime? CompletedAtUtc { get; private set; }
        public long Version { get; private set; }

        public IReadOnlyCollection<PaymentAttempt> Attempts => _attempts.AsReadOnly();
        public IReadOnlyCollection<TransactionStatusHistoryEntry> StatusHistory => _statusHistory.AsReadOnly();

        public static PaymentTransaction Create(
            string reference,
            string? idempotencyKey,
            string? clientReference,
            string? customerId,
            long amountMinor,
            string currency,
            string? description,
            DateTime createdAtUtc)
        {
            return new PaymentTransaction(
                Guid.NewGuid(),
                reference,
                idempotencyKey,
                clientReference,
                customerId,
                amountMinor,
                currency,
                description,
                createdAtUtc);
        }

        public void MarkProcessing(
            PaymentProvider provider,
            RoutingDecisionType routingDecisionType,
            DateTime updatedAtUtc)
        {
            SelectedProvider = provider;
            RoutingDecisionType = routingDecisionType;
            ProcessedAtUtc = updatedAtUtc;
            TransitionStatus(TransactionStatus.Processing, "Transaction is being processed.", "orchestrator", updatedAtUtc);
        }

        public void MarkSucceeded(
            PaymentProvider provider,
            DateTime updatedAtUtc)
        {
            SelectedProvider = provider;
            FailureCategory = null;
            FailureReason = null;
            CompletedAtUtc = updatedAtUtc;
            TransitionStatus(TransactionStatus.Succeeded, "Transaction completed successfully.", "provider", updatedAtUtc);
        }

        public void MarkFailed(
            PaymentProvider provider,
            FailureCategory failureCategory,
            string failureReason,
            DateTime updatedAtUtc)
        {
            if (string.IsNullOrWhiteSpace(failureReason))
                throw new ArgumentException("Failure reason is required.", nameof(failureReason));

            SelectedProvider = provider;
            FailureCategory = failureCategory;
            FailureReason = failureReason.Trim();
            CompletedAtUtc = updatedAtUtc;
            TransitionStatus(TransactionStatus.Failed, failureReason, "provider", updatedAtUtc);
        }

        public void ScheduleRetry(
            PaymentProvider provider,
            FailureCategory failureCategory,
            string failureReason,
            DateTime updatedAtUtc)
        {
            if (string.IsNullOrWhiteSpace(failureReason))
                throw new ArgumentException("Failure reason is required.", nameof(failureReason));

            SelectedProvider = provider;
            FailureCategory = failureCategory;
            FailureReason = failureReason.Trim();
            TransitionStatus(TransactionStatus.Retrying, failureReason, "retry-engine", updatedAtUtc);
        }

        public PaymentAttempt AddAttempt(
            PaymentProvider provider,
            bool isRetry,
            bool isFailover,
            DateTime startedAtUtc)
        {
            var nextAttemptNumber = _attempts.Count + 1;

            var attempt = PaymentAttempt.Create(
                paymentTransactionId: Id,
                attemptNumber: nextAttemptNumber,
                provider: provider,
                isRetry: isRetry,
                isFailover: isFailover,
                startedAtUtc: startedAtUtc);

            _attempts.Add(attempt);
            Touch(startedAtUtc);

            return attempt;
        }

        private void TransitionStatus(
            TransactionStatus newStatus,
            string reason,
            string source,
            DateTime updatedAtUtc)
        {
            if (Status == newStatus)
            {
                Touch(updatedAtUtc);
                return;
            }

            var previousStatus = Status;
            Status = newStatus;
            Touch(updatedAtUtc);

            AddStatusHistory(previousStatus, newStatus, reason, source, updatedAtUtc);
        }

        private void AddStatusHistory(
            TransactionStatus? previousStatus,
            TransactionStatus newStatus,
            string? reason,
            string source,
            DateTime changedAtUtc)
        {
            _statusHistory.Add(TransactionStatusHistoryEntry.Create(
                paymentTransactionId: Id,
                previousStatus: previousStatus,
                newStatus: newStatus,
                reason: reason,
                source: source,
                changedAtUtc: changedAtUtc));
        }

        private void Touch(DateTime updatedAtUtc)
        {
            UpdatedAtUtc = updatedAtUtc;
            Version++;
        }
    }
}
