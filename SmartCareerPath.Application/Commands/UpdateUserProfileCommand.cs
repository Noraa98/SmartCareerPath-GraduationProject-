using MediatR;
using SmartCareerPath.Application.Abstraction.DTOs.User;

namespace SmartCareerPath.Application.Commands
{
    public class UpdateUserProfileCommand : IRequest<BaseResponse<UserProfileDto>>
    {
        public int UserId { get; set; }
        public string Bio { get; set; }
        public string CurrentRole { get; set; }
        public int? ExperienceYears { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string LinkedInUrl { get; set; }
        public string GithubUrl { get; set; }
        public string PortfolioUrl { get; set; }
    }
}
