using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.ProfileAndInterests
{
    public class UserInterest : BaseEntity
    {
        [Required]
        public int UserProfileId { get; set; }
        public UserProfile UserProfile { get; set; }

        [Required]
        public int InterestId { get; set; }
        public Interest Interest { get; set; }

        public int? MatchPercentage { get; set; }

        public DateTime SelectedAt { get; set; } = DateTime.UtcNow;
    }
}
