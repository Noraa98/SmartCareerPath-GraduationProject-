using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Commands
{
    public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
    {
        public UpdateUserProfileCommandValidator()
        {
            RuleFor(x => x.UserId).GreaterThan(0);
            RuleFor(x => x.Bio).MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Bio));
            RuleFor(x => x.CurrentRole).MaximumLength(200).When(x => !string.IsNullOrEmpty(x.CurrentRole));
            RuleFor(x => x.ExperienceYears).GreaterThanOrEqualTo(0).When(x => x.ExperienceYears.HasValue);
        }
    }

}
