using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Domain.Entities.CareerPath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.CareerPath
{
    public class CareerPathSkillConfiguration : IEntityTypeConfiguration<CareerPathSkill>
    {
        public void Configure(EntityTypeBuilder<CareerPathSkill> builder)
        {
            builder.ToTable("CareerPathSkills");

            builder.HasKey(cs => cs.Id);

            builder.HasIndex(cs => new { cs.CareerPathId, cs.SkillId })
                .IsUnique();

            builder.HasOne(cs => cs.CareerPath)
                .WithMany(c => c.RequiredSkills)
                .HasForeignKey(cs => cs.CareerPathId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cs => cs.Skill)
                .WithMany(s => s.CareerPathSkills)
                .HasForeignKey(cs => cs.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
