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
    public class CareerPathStepConfiguration : IEntityTypeConfiguration<CareerPathStep>
    {
        public void Configure(EntityTypeBuilder<CareerPathStep> builder)
        {
            builder.ToTable("CareerPathSteps");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasOne(s => s.CareerPath)
                .WithMany(c => c.Steps)
                .HasForeignKey(s => s.CareerPathId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
