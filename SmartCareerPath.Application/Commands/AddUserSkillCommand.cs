using MediatR;
using SmartCareerPath.Application.Abstraction.DTOs.User;

namespace SmartCareerPath.Application.Commands
{
    public class AddUserSkillCommand : IRequest<BaseResponse<UserSkillDto>>
    {
        public int UserId { get; set; }
        public int SkillId { get; set; }
        public int ProficiencyLevel { get; set; }
        public string Source { get; set; } = "Manual";
    }
}
