using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Domain.Entities.SkillManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.Skill
{
    public class JobSkillConfiguration : IEntityTypeConfiguration<JobSkill>
    {
        public void Configure(EntityTypeBuilder<JobSkill> builder)
        {
            builder.ToTable("JobSkills");

            builder.HasKey(js => js.Id);

            builder.HasIndex(js => new { js.JobPostingId, js.SkillId })
                .IsUnique();

            builder.HasOne(js => js.JobPosting)
                .WithMany(j => j.RequiredSkills)
                .HasForeignKey(js => js.JobPostingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(js => js.Skill)
                .WithMany(s => s.JobSkills)
                .HasForeignKey(js => js.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
