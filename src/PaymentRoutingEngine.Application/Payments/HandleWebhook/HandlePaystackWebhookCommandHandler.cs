using Microsoft.Extensions.Configuration;
using PaymentRoutingEngine.Application.Abstractions.Common;
using PaymentRoutingEngine.Application.Abstractions.Messaging;
using PaymentRoutingEngine.Application.Abstractions.Persistence;
using PaymentRoutingEngine.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PaymentRoutingEngine.Application.Payments.HandleWebhook
{
    public sealed class HandlePaystackWebhookCommandHandler
    : IRequestHandler<HandlePaystackWebhookCommand, Unit>
    {
        private readonly IConfiguration _configuration;
        private readonly IPaymentTransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;

        public HandlePaystackWebhookCommandHandler(
            IConfiguration configuration,
            IPaymentTransactionRepository transactionRepository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTimeProvider)
        {
            _configuration = configuration;
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Unit> Handle(
            HandlePaystackWebhookCommand request,
            CancellationToken cancellationToken)
        {
            VerifySignature(request.Payload, request.Signature);

            var json = JsonDocument.Parse(request.Payload);

            var eventType = json.RootElement.GetProperty("event").GetString();

            if (eventType != "charge.success")
                return Unit.Value;

            var data = json.RootElement.GetProperty("data");

            var reference = data.GetProperty("reference").GetString();

            var transaction = await _transactionRepository
                .GetByReferenceAsync(reference!, cancellationToken);

            if (transaction is null)
                return Unit.Value; // silently ignore

            if (transaction.Status == TransactionStatus.Succeeded)
                return Unit.Value;

            transaction.MarkSucceeded(
                provider: PaymentProvider.Paystack,
                updatedAtUtc: _dateTimeProvider.UtcNow);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }

        private void VerifySignature(string payload, string? signature)
        {
            if (string.IsNullOrWhiteSpace(signature))
                throw new UnauthorizedAccessException("Missing Paystack signature.");

            var secret = _configuration["Paystack:WebhookSecret"];
            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException("Webhook secret not configured.");

            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret!));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = BitConverter.ToString(hash).Replace("-", "").ToLower();

            if (computedSignature != signature.ToLower())
                throw new UnauthorizedAccessException("Invalid Paystack signature.");
        }
    }
}
