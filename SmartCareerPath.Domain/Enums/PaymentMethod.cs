using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Domain.Enums
{
    public enum PaymentMethod
    {
        CreditCard = 1,
        DebitCard = 2,
        PayPalWallet = 3,
        MobileWallet = 4,    // Vodafone Cash, etc.
        BankTransfer = 5,
        ApplePay = 6,
        GooglePay = 7,
        Unknown = 99
    }
}
