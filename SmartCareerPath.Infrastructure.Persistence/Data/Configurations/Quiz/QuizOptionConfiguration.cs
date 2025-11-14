using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Domain.Entities.Quiz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data.Configurations.Quiz
{
    public class QuizOptionConfiguration : IEntityTypeConfiguration<QuizOption>
    {
        public void Configure(EntityTypeBuilder<QuizOption> builder)
        {
            builder.ToTable("QuizOptions");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.OptionText)
                .IsRequired();

            builder.HasOne(o => o.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
