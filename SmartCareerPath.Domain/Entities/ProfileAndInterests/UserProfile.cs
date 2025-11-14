using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.Auth;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.ProfileAndInterests
{
    public class UserProfile : BaseEntity
    {
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        public string Bio { get; set; }

        [MaxLength(200)]
        public string CurrentRole { get; set; }

        public int? ExperienceYears { get; set; }

        [MaxLength(200)]
        public string ProfilePictureUrl { get; set; }

        [MaxLength(200)]
        public string City { get; set; }

        [MaxLength(100)]
        public string Country { get; set; }

        [MaxLength(200)]
        public string LinkedInUrl { get; set; }

        [MaxLength(200)]
        public string GithubUrl { get; set; }

        [MaxLength(200)]
        public string PortfolioUrl { get; set; }

        // Navigation
        public ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();
    }
}
