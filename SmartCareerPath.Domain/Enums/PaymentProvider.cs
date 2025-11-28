using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Domain.Enums
{
    public enum PaymentProvider
    {
        Stripe = 1,
        PayPal = 2,
        Paymob = 3,
        Manual = 99
    }

}
