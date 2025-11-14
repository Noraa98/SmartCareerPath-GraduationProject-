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
    public class QuizSkillConfiguration : IEntityTypeConfiguration<QuizSkill>
    {
        public void Configure(EntityTypeBuilder<QuizSkill> builder)
        {
            builder.ToTable("QuizSkills");

            builder.HasKey(qs => qs.Id);

            builder.HasIndex(qs => new { qs.QuizId, qs.SkillId })
                .IsUnique();

            builder.HasOne(qs => qs.Quiz)
                .WithMany(q => q.Skills)
                .HasForeignKey(qs => qs.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(qs => qs.Skill)
                .WithMany(s => s.QuizSkills)
                .HasForeignKey(qs => qs.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
