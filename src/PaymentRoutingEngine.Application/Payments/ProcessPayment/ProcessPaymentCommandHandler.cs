using PaymentRoutingEngine.Application.Abstractions.Common;
using PaymentRoutingEngine.Application.Abstractions.Messaging;
using PaymentRoutingEngine.Application.Abstractions.Payments;
using PaymentRoutingEngine.Application.Abstractions.Persistence;
using PaymentRoutingEngine.Application.Abstractions.Providers;
using PaymentRoutingEngine.Domain.Entities;
using PaymentRoutingEngine.Domain.Enums;
using System.Text.Json;

namespace PaymentRoutingEngine.Application.Payments.ProcessPayment;

public sealed class ProcessPaymentCommandHandler
    : IRequestHandler<ProcessPaymentCommand, ProcessPaymentResult>
{
    private const int MaxRetryAttemptsPerProvider = 2;

    private readonly IPaymentTransactionRepository _paymentTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPaymentProviderResolver _providerResolver;
    private readonly IPaymentProviderRoutingService _providerRoutingService;
    private readonly IPaymentRetryPolicy _retryPolicy;

    public ProcessPaymentCommandHandler(
        IPaymentTransactionRepository paymentTransactionRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        IPaymentProviderResolver providerResolver,
        IPaymentProviderRoutingService providerRoutingService,
        IPaymentRetryPolicy retryPolicy)
    {
        _paymentTransactionRepository = paymentTransactionRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _providerResolver = providerResolver;
        _providerRoutingService = providerRoutingService;
        _retryPolicy = retryPolicy;
    }

    public async Task<ProcessPaymentResult> Handle(
        ProcessPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var transaction = await _paymentTransactionRepository
            .GetByIdAsync(request.TransactionId, cancellationToken);

        if (transaction is null)
            throw new KeyNotFoundException("Transaction not found.");

        if (transaction.Status != TransactionStatus.Pending)
            throw new InvalidOperationException("Only pending transactions can be processed.");

        var primaryProvider = _providerRoutingService.GetPrimaryProvider(transaction);

        var result = await TryProviderWithRetriesAsync(
            transaction,
            primaryProvider,
            isFailover: false,
            cancellationToken);

        if (!result.IsSuccessful && _retryPolicy.IsRetryable(result.FailureCategory))
        {
            var failoverProviders = _providerRoutingService.GetFailoverProviders(
                transaction,
                primaryProvider);

            foreach (var failoverProvider in failoverProviders)
            {
                result = await TryProviderWithRetriesAsync(
                    transaction,
                    failoverProvider,
                    isFailover: true,
                    cancellationToken);

                if (result.IsSuccessful)
                    break;

                if (!_retryPolicy.IsRetryable(result.FailureCategory))
                    break;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProcessPaymentResult(
            TransactionId: transaction.Id,
            Reference: transaction.Reference,
            Status: transaction.Status,
            FailureReason: transaction.FailureReason,
            ProviderReference: result.ProviderReference);
    }

    private async Task<ProviderExecutionResult> TryProviderWithRetriesAsync(
        PaymentTransaction transaction,
        PaymentProvider provider,
        bool isFailover,
        CancellationToken cancellationToken)
    {
        ProviderExecutionResult? lastResult = null;

        for (var attemptIndex = 1; attemptIndex <= MaxRetryAttemptsPerProvider; attemptIndex++)
        {
            var isRetry = attemptIndex > 1;
            var now = _dateTimeProvider.UtcNow;

            transaction.MarkProcessing(
                provider: provider,
                routingDecisionType: isFailover
                    ? RoutingDecisionType.Failover
                    : isRetry
                        ? RoutingDecisionType.Retry
                        : RoutingDecisionType.Primary,
                updatedAtUtc: now);

            var attempt = transaction.AddAttempt(
                provider: provider,
                isRetry: isRetry,
                isFailover: isFailover,
                startedAtUtc: now);

            var providerRequest = new ProviderPaymentRequest
            {
                Reference = transaction.Reference,
                AmountMinor = transaction.AmountMinor,
                Currency = transaction.Currency,
                CustomerId = transaction.CustomerId,
                Description = transaction.Description
            };

            attempt.AttachRequestPayload(JsonSerializer.Serialize(providerRequest));

            var providerClient = _providerResolver.Resolve(provider);

            var providerResponse = await providerClient.ProcessPaymentAsync(
                providerRequest,
                cancellationToken);

            if (providerResponse.IsSuccessful)
            {
                attempt.MarkSucceeded(
                    providerReference: providerResponse.ProviderReference,
                    providerStatusCode: providerResponse.ProviderStatusCode,
                    responsePayload: providerResponse.RawResponsePayload,
                    completedAtUtc: _dateTimeProvider.UtcNow);

                transaction.MarkSucceeded(
                    provider: provider,
                    updatedAtUtc: _dateTimeProvider.UtcNow);

                return ProviderExecutionResult.Success(providerResponse.ProviderReference);
            }

            var failureCategory = providerResponse.FailureCategory ?? FailureCategory.Unknown;
            var failureReason = providerResponse.FailureReason ?? "Payment provider failed.";

            attempt.MarkFailed(
                failureCategory: failureCategory,
                failureReason: failureReason,
                providerReference: providerResponse.ProviderReference,
                providerStatusCode: providerResponse.ProviderStatusCode,
                responsePayload: providerResponse.RawResponsePayload,
                completedAtUtc: _dateTimeProvider.UtcNow);

            lastResult = ProviderExecutionResult.Failure(
                failureCategory,
                failureReason,
                providerResponse.ProviderReference);

            if (!_retryPolicy.IsRetryable(failureCategory))
            {
                transaction.MarkFailed(
                    provider: provider,
                    failureCategory: failureCategory,
                    failureReason: failureReason,
                    updatedAtUtc: _dateTimeProvider.UtcNow);

                return lastResult;
            }

            if (attemptIndex < MaxRetryAttemptsPerProvider)
            {
                transaction.ScheduleRetry(
                    provider: provider,
                    failureCategory: failureCategory,
                    failureReason: failureReason,
                    updatedAtUtc: _dateTimeProvider.UtcNow);
            }
        }

        var finalFailure = lastResult ?? ProviderExecutionResult.Failure(
            FailureCategory.Unknown,
            "Payment processing failed.",
            null);

        transaction.MarkFailed(
            provider: provider,
            failureCategory: finalFailure.FailureCategory,
            failureReason: finalFailure.FailureReason ?? "Payment processing failed.",
            updatedAtUtc: _dateTimeProvider.UtcNow);

        return finalFailure;
    }

    private sealed record ProviderExecutionResult(
        bool IsSuccessful,
        FailureCategory FailureCategory,
        string? FailureReason,
        string? ProviderReference)
    {
        public static ProviderExecutionResult Success(string? providerReference)
            => new(
                IsSuccessful: true,
                FailureCategory: FailureCategory.None,
                FailureReason: null,
                ProviderReference: providerReference);

        public static ProviderExecutionResult Failure(
            FailureCategory failureCategory,
            string failureReason,
            string? providerReference)
            => new(
                IsSuccessful: false,
                FailureCategory: failureCategory,
                FailureReason: failureReason,
                ProviderReference: providerReference);
    }
}