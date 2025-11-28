using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Domain.Enums
{
    public enum BillingCycle
    {
        Monthly = 1,
        Yearly = 2,
        Lifetime = 3,        // One-time payment, no renewal
        PayPerUse = 4        // No subscription, pay as you go
    }
}
