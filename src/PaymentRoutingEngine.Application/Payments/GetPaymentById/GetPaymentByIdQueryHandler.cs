using PaymentRoutingEngine.Application.Abstractions.Messaging;
using PaymentRoutingEngine.Application.Abstractions.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Payments.GetPaymentById
{
    public sealed class GetPaymentByIdQueryHandler
    : IRequestHandler<GetPaymentByIdQuery, GetPaymentByIdResult>
    {
        private readonly IPaymentTransactionRepository _repository;

        public GetPaymentByIdQueryHandler(IPaymentTransactionRepository repository)
        {
            _repository = repository;
        }

        public async Task<GetPaymentByIdResult> Handle(
            GetPaymentByIdQuery request,
            CancellationToken cancellationToken)
        {
            var transaction = await _repository.GetByIdAsync(request.TransactionId, cancellationToken);

            if (transaction is null)
                throw new KeyNotFoundException("Transaction not found.");

            return new GetPaymentByIdResult(
                TransactionId: transaction.Id,
                Reference: transaction.Reference,
                IdempotencyKey: transaction.IdempotencyKey,
                ClientReference: transaction.ClientReference,
                CustomerId: transaction.CustomerId,
                AmountMinor: transaction.AmountMinor,
                Currency: transaction.Currency,
                Status: transaction.Status,
                FailureReason: transaction.FailureReason,
                CreatedAtUtc: transaction.CreatedAtUtc,
                CompletedAtUtc: transaction.CompletedAtUtc
            );
        }
    }
}
