using SmartCareerPath.Domain.Common.ResultPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Domain.Common.ValueObjects
{
    public class Email : ValueObject
    {
        public string Value { get; private set; }

        private Email(string value)
        {
            Value = value;
        }

        public static Result<Email> Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result<Email>.Failure("Email cannot be empty");

            if (!email.Contains("@"))
                return Result<Email>.Failure("Invalid email format");

            return Result<Email>.Success(new Email(email));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

}
