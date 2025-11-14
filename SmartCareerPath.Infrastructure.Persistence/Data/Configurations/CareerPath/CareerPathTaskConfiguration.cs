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
    public class CareerPathTaskConfiguration : IEntityTypeConfiguration<CareerPathTask>
    {
        public void Configure(EntityTypeBuilder<CareerPathTask> builder)
        {
            builder.ToTable("CareerPathTasks");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasOne(t => t.Step)
                .WithMany(s => s.Tasks)
                .HasForeignKey(t => t.StepId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
