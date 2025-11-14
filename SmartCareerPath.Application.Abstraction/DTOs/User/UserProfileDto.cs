using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.DTOs.User
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Bio { get; set; }
        public string CurrentRole { get; set; }
        public int? ExperienceYears { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string LinkedInUrl { get; set; }
        public string GithubUrl { get; set; }
        public string PortfolioUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<UserInterestDto> Interests { get; set; }
        public List<UserSkillDto> Skills { get; set; }
    }
}
