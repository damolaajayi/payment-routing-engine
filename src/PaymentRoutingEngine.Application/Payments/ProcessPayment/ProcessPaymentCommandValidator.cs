using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Payments.ProcessPayment
{
    public sealed class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
    {
        public ProcessPaymentCommandValidator()
        {
            RuleFor(x => x.TransactionId)
                .NotEmpty()
                .WithMessage("Transaction id is required.");
        }
    }
}
