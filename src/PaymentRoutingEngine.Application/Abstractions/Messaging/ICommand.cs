using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Abstractions.Messaging
{
    public interface ICommand<TResponse> : IRequest<TResponse>
    {
    }
}
