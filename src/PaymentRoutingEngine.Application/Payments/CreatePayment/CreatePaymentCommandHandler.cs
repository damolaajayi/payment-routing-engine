using PaymentRoutingEngine.Application.Abstractions.Common;
using PaymentRoutingEngine.Application.Abstractions.Messaging;
using PaymentRoutingEngine.Application.Abstractions.Persistence;
using PaymentRoutingEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace PaymentRoutingEngine.Application.Payments.CreatePayment
{
    public sealed class CreatePaymentCommandHandler
    : IRequestHandler<CreatePaymentCommand, CreatePaymentResult>
    {
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CreatePaymentCommandHandler(
            IPaymentTransactionRepository paymentTransactionRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTimeProvider)
        {
            _paymentTransactionRepository = paymentTransactionRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<CreatePaymentResult> Handle(
            CreatePaymentCommand request,
            CancellationToken cancellationToken)
        {

            if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                var existingTransaction =
                    await _paymentTransactionRepository.GetByIdempotencyKeyAsync(
                        request.IdempotencyKey,
                        cancellationToken);

                if (existingTransaction is not null)
                {
                    return new CreatePaymentResult(
                        TransactionId: existingTransaction.Id,
                        Reference: existingTransaction.Reference,
                        IdempotencyKey: existingTransaction.IdempotencyKey,
                        AmountMinor: existingTransaction.AmountMinor,
                        Currency: existingTransaction.Currency,
                        Status: existingTransaction.Status,
                        CreatedAtUtc: existingTransaction.CreatedAtUtc);
                }
            }

            var reference = GenerateReference();

            var transaction = PaymentTransaction.Create(
                reference: reference,
                idempotencyKey: request.IdempotencyKey,
                clientReference: request.ClientReference,
                customerId: request.CustomerId,
                amountMinor: request.AmountMinor,
                currency: request.Currency,
                description: request.Description,
                createdAtUtc: _dateTimeProvider.UtcNow);

            await _paymentTransactionRepository.AddAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreatePaymentResult(
                TransactionId: transaction.Id,
                Reference: transaction.Reference,
                IdempotencyKey: transaction.IdempotencyKey,
                AmountMinor: transaction.AmountMinor,
                Currency: transaction.Currency,
                Status: transaction.Status,
                CreatedAtUtc: transaction.CreatedAtUtc);
        }

        private static string GenerateReference()
        {
            return $"PAY-{Guid.NewGuid():N[..12].ToUpper()}";
        }
    }
}
