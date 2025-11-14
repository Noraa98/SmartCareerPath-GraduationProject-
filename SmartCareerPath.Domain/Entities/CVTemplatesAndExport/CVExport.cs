using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.ResumeAndParsing;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.CVTemplatesAndExport
{
    public class CVExport : BaseEntity
    {
        [Required]
        public int ResumeId { get; set; }
        public Resume Resume { get; set; }

        public int? TemplateId { get; set; }
        public CVTemplate Template { get; set; }

        [MaxLength(500)]
        public string FilePath { get; set; }

        [MaxLength(50)]
        public string FileFormat { get; set; }

        public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
    }

}
