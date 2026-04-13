using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Domain.Entities
{
    public sealed class PaymentAttempt
    {
        private PaymentAttempt()
        {
        }

        private PaymentAttempt(
            Guid id,
            Guid paymentTransactionId,
            int attemptNumber,
            PaymentProvider provider,
            bool isRetry,
            bool isFailover,
            DateTime startedAtUtc)
        {
            if (attemptNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(attemptNumber), "Attempt number must be greater than zero.");

            Id = id;
            PaymentTransactionId = paymentTransactionId;
            AttemptNumber = attemptNumber;
            Provider = provider;
            Status = AttemptStatus.Processing;
            IsRetry = isRetry;
            IsFailover = isFailover;
            StartedAtUtc = startedAtUtc;
            CreatedAtUtc = startedAtUtc;
        }

        public Guid Id { get; private set; }
        public Guid PaymentTransactionId { get; private set; }
        public int AttemptNumber { get; private set; }
        public PaymentProvider Provider { get; private set; }
        public AttemptStatus Status { get; private set; }
        public string? ProviderReference { get; private set; }
        public string? ProviderStatusCode { get; private set; }
        public FailureCategory? FailureCategory { get; private set; }
        public string? FailureReason { get; private set; }
        public bool IsRetry { get; private set; }
        public bool IsFailover { get; private set; }
        public string? RequestPayload { get; private set; }
        public string? ResponsePayload { get; private set; }
        public DateTime StartedAtUtc { get; private set; }
        public DateTime? CompletedAtUtc { get; private set; }
        public DateTime? NextRetryAtUtc { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }

        public static PaymentAttempt Create(
            Guid paymentTransactionId,
            int attemptNumber,
            PaymentProvider provider,
            bool isRetry,
            bool isFailover,
            DateTime startedAtUtc)
        {
            return new PaymentAttempt(
                Guid.NewGuid(),
                paymentTransactionId,
                attemptNumber,
                provider,
                isRetry,
                isFailover,
                startedAtUtc);
        }

        public void AttachRequestPayload(string? requestPayload)
        {
            RequestPayload = requestPayload;
        }

        public void MarkSucceeded(
            string? providerReference,
            string? providerStatusCode,
            string? responsePayload,
            DateTime completedAtUtc)
        {
            ProviderReference = providerReference;
            ProviderStatusCode = providerStatusCode;
            ResponsePayload = responsePayload;
            Status = AttemptStatus.Succeeded;
            CompletedAtUtc = completedAtUtc;
            FailureCategory = null;
            FailureReason = null;
            NextRetryAtUtc = null;
        }

        public void MarkFailed(
            FailureCategory failureCategory,
            string failureReason,
            string? providerReference,
            string? providerStatusCode,
            string? responsePayload,
            DateTime completedAtUtc,
            DateTime? nextRetryAtUtc = null)
        {
            if (string.IsNullOrWhiteSpace(failureReason))
                throw new ArgumentException("Failure reason is required.", nameof(failureReason));

            FailureCategory = failureCategory;
            FailureReason = failureReason.Trim();
            ProviderReference = providerReference;
            ProviderStatusCode = providerStatusCode;
            ResponsePayload = responsePayload;
            Status = AttemptStatus.Failed;
            CompletedAtUtc = completedAtUtc;
            NextRetryAtUtc = nextRetryAtUtc;
        }
    }
}
