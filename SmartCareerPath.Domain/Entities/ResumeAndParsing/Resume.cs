using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.Auth;
using SmartCareerPath.Domain.Entities.JobPostingAndMatching;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.ResumeAndParsing
{
    public class Resume : BaseEntity
    {
        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [MaxLength(200)]
        public string Title { get; set; }

        public string Content { get; set; }

        [MaxLength(500)]
        public string FileUrl { get; set; }

        [MaxLength(50)]
        public string FileType { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsPrimary { get; set; }

        // Navigation
        public ResumeParsingResult ParsingResult { get; set; }
        public ICollection<ResumeKeyword> Keywords { get; set; } = new List<ResumeKeyword>();
        public ICollection<ResumeSuggestion> Suggestions { get; set; } = new List<ResumeSuggestion>();
        public ICollection<ResumeScore> Scores { get; set; } = new List<ResumeScore>();
        public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
    }

}
