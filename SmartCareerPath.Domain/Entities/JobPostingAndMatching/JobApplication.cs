using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.Auth;
using SmartCareerPath.Domain.Entities.ResumeAndParsing;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.JobPostingAndMatching
{
    public class JobApplication : BaseEntity
    {
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int JobPostingId { get; set; }
        public JobPosting JobPosting { get; set; }

        public int? ResumeId { get; set; }
        public Resume Resume { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; }

        public int? MatchScore { get; set; }

        public string CoverLetter { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        public string ReviewNotes { get; set; }

        public int? ReviewedByUserId { get; set; }

        // Navigation
        public ICollection<ApplicationStatusHistory> StatusHistory { get; set; } = new List<ApplicationStatusHistory>();
    }
}
