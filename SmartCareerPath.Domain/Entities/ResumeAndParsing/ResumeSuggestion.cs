using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.ResumeAndParsing
{
    public class ResumeSuggestion : BaseEntity
    {
        [Required]
        public int ResumeId { get; set; }
        public Resume Resume { get; set; }

        [MaxLength(100)]
        public string Category { get; set; }

        [Required]
        public string Suggestion { get; set; }

        public int Priority { get; set; }

        public bool IsImplemented { get; set; }
    }
}
