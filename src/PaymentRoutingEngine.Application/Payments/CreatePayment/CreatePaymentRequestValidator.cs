using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Payments.CreatePayment
{
    public sealed class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
    {
        public CreatePaymentCommandValidator()
        {
            RuleFor(x => x.AmountMinor)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .Length(3)
                .WithMessage("Currency must be a valid 3-letter code.");

            RuleFor(x => x.IdempotencyKey)
                .NotEmpty()
                .When(x => x.ClientReference != null);

            RuleFor(x => x.ClientReference)
                .MaximumLength(100);

            RuleFor(x => x.CustomerId)
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .MaximumLength(500);
        }
    }
}
