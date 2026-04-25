using System.Text.Json;
using PaymentRoutingEngine.Application.Abstractions.Common;
using PaymentRoutingEngine.Application.Abstractions.Messaging;
using PaymentRoutingEngine.Application.Abstractions.Persistence;
using PaymentRoutingEngine.Application.Abstractions.Providers;
using PaymentRoutingEngine.Domain.Enums;

namespace PaymentRoutingEngine.Application.Payments.ProcessPayment;

public sealed class ProcessPaymentCommandHandler
    : IRequestHandler<ProcessPaymentCommand, ProcessPaymentResult>
{
    private readonly IPaymentTransactionRepository _paymentTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPaymentProviderResolver _providerResolver;

    public ProcessPaymentCommandHandler(
        IPaymentTransactionRepository paymentTransactionRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        IPaymentProviderResolver providerResolver)
    {
        _paymentTransactionRepository = paymentTransactionRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _providerResolver = providerResolver;
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

        var provider = PaymentProvider.MockProvider;
        var now = _dateTimeProvider.UtcNow;

        var providerClient = _providerResolver.Resolve(provider);

        transaction.MarkProcessing(
            provider: provider,
            routingDecisionType: RoutingDecisionType.Primary,
            updatedAtUtc: now);

        var attempt = transaction.AddAttempt(
            provider: provider,
            isRetry: false,
            isFailover: false,
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

        var providerResponse = await providerClient.ProcessPaymentAsync(providerRequest, cancellationToken);

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
        }
        else
        {
            var failureCategory = providerResponse.FailureCategory ?? FailureCategory.Unknown;
            var failureReason = providerResponse.FailureReason ?? "Payment processing failed.";

            attempt.MarkFailed(
                failureCategory: failureCategory,
                failureReason: failureReason,
                providerReference: providerResponse.ProviderReference,
                providerStatusCode: providerResponse.ProviderStatusCode,
                responsePayload: providerResponse.RawResponsePayload,
                completedAtUtc: _dateTimeProvider.UtcNow);

            transaction.MarkFailed(
                provider: provider,
                failureCategory: failureCategory,
                failureReason: failureReason,
                updatedAtUtc: _dateTimeProvider.UtcNow);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProcessPaymentResult(
            TransactionId: transaction.Id,
            Reference: transaction.Reference,
            Status: transaction.Status,
            FailureReason: transaction.FailureReason,
            ProviderReference: providerResponse.ProviderReference);
    }
}