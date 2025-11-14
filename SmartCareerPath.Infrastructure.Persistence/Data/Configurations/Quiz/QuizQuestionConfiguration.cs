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
    public class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
    {
        public void Configure(EntityTypeBuilder<QuizQuestion> builder)
        {
            builder.ToTable("QuizQuestions");

            builder.HasKey(q => q.Id);

            builder.Property(q => q.QuestionText)
                .IsRequired();

            builder.Property(q => q.QuestionType)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
