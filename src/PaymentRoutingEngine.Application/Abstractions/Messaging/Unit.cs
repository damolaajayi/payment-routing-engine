using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentRoutingEngine.Application.Abstractions.Messaging
{
    public readonly struct Unit
    {
        public static readonly Unit Value = new();
    }
}
