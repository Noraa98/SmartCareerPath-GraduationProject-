using SmartCareerPath.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Domain.ValueObjects
{
    public sealed class Money : IEquatable<Money>
    {
        public decimal Amount { get; }
        public Currency Currency { get; }

        // Private constructor to enforce factory methods
        private Money(decimal amount, Currency currency)
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative", nameof(amount));

            Amount = Math.Round(amount, 2); // Always 2 decimal places for money
            Currency = currency;
        }

       
        // Factory Methods
        public static Money Create(decimal amount, Currency currency)
        {
            return new Money(amount, currency);
        }

        public static Money USD(decimal amount) => new(amount, Currency.USD);

        
        public static Money EGP(decimal amount) => new(amount, Currency.EGP);

       
        public static Money EUR(decimal amount) => new(amount, Currency.EUR);

        
        public static Money Zero(Currency currency) => new(0, currency);

       
        // Operations (must be same currency)
        public Money Add(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException($"Cannot add {Currency} and {other.Currency}");

            return new Money(Amount + other.Amount, Currency);
        }

      
        public Money Subtract(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException($"Cannot subtract {Currency} and {other.Currency}");

            return new Money(Amount - other.Amount, Currency);
        }

      
        public Money Multiply(decimal factor)
        {
            return new Money(Amount * factor, Currency);
        }

     
        public Money Percentage(decimal percentage)
        {
            return new Money(Amount * (percentage / 100), Currency);
        }

        
        // Comparison
        public bool IsZero => Amount == 0;
        public bool IsPositive => Amount > 0;
        public bool IsNegative => Amount < 0;

        public bool GreaterThan(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("Cannot compare different currencies");
            return Amount > other.Amount;
        }

        public bool LessThan(Money other)
        {
            if (Currency != other.Currency)
                throw new InvalidOperationException("Cannot compare different currencies");
            return Amount < other.Amount;
        }

   
        // Value Object Equality (by value, not reference)
        public bool Equals(Money? other)
        {
            if (other is null) return false;
            return Amount == other.Amount && Currency == other.Currency;
        }

        public override bool Equals(object? obj)
        {
            return obj is Money other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Amount, Currency);
        }

        public static bool operator ==(Money? left, Money? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(Money? left, Money? right)
        {
            return !(left == right);
        }

     
        // String Representation
        public override string ToString()
        {
            return $"{Amount:F2} {Currency}";
        }

        public string ToDisplayString()
        {
            return Currency switch
            {
                Currency.USD => $"${Amount:F2}",
                Currency.EUR => $"€{Amount:F2}",
                Currency.GBP => $"£{Amount:F2}",
                Currency.EGP => $"{Amount:F2} EGP",
                Currency.SAR => $"{Amount:F2} SAR",
                _ => ToString()
            };
        }
    }
}
