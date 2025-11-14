using SmartCareerPath.Domain.Common.BaseEntities;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.ResumeAndParsing
{
    public class ResumeParsingResult : BaseEntity
    {
        [Required]
        public int ResumeId { get; set; }
        public Resume Resume { get; set; }

        public string ParsedJson { get; set; }

        public DateTime ParsedAt { get; set; } = DateTime.UtcNow;

        public bool IsSuccessful { get; set; }

        public string ErrorMessage { get; set; }
    }
}
