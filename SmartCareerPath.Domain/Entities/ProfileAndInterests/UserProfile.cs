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

        public string? Bio { get; set; } = string.Empty;

        [MaxLength(200)]
        public string CurrentRole { get; set; } = string.Empty;

        public int? ExperienceYears { get; set; }

        [MaxLength(200)]
        public string ProfilePictureUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? City { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Country { get; set; } = string.Empty;

        [MaxLength(200)]
        public string LinkedInUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string GithubUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string PortfolioUrl { get; set; } = string.Empty;

        // Navigation
        public ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();

    }
}
