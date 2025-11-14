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
    public class SkillGapConfiguration : IEntityTypeConfiguration<SkillGap>
    {
        public void Configure(EntityTypeBuilder<SkillGap> builder)
        {
            builder.ToTable("SkillGaps");

            builder.HasKey(sg => sg.Id);

            builder.HasOne(sg => sg.UserCareerPath)
                .WithMany(uc => uc.SkillGaps)
                .HasForeignKey(sg => sg.UserCareerPathId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sg => sg.Skill)
                .WithMany()
                .HasForeignKey(sg => sg.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
