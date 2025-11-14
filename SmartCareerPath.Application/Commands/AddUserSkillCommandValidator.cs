using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Commands
{
    public class AddUserSkillCommandValidator : AbstractValidator<AddUserSkillCommand>
    {
        public AddUserSkillCommandValidator()
        {
            RuleFor(x => x.UserId).GreaterThan(0);
            RuleFor(x => x.SkillId).GreaterThan(0);
            RuleFor(x => x.ProficiencyLevel).InclusiveBetween(1, 5);
        }
    }
}
