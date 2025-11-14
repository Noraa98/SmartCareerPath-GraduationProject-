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
    public class QuizAnswerConfiguration : IEntityTypeConfiguration<QuizAnswer>
    {
        public void Configure(EntityTypeBuilder<QuizAnswer> builder)
        {
            builder.ToTable("QuizAnswers");

            builder.HasKey(a => a.Id);

            builder.HasOne(a => a.QuizSession)
                .WithMany(s => s.Answers)
                .HasForeignKey(a => a.QuizSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
