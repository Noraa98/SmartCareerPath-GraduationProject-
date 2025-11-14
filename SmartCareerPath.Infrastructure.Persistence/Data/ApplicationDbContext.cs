using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.AIEngine;
using SmartCareerPath.Domain.Entities.Auth;
using SmartCareerPath.Domain.Entities.CareerPath;
using SmartCareerPath.Domain.Entities.CVTemplatesAndExport;
using SmartCareerPath.Domain.Entities.InterviewSystem;
using SmartCareerPath.Domain.Entities.JobPostingAndMatching;
using SmartCareerPath.Domain.Entities.NotificationsLogsAndConfig;
using SmartCareerPath.Domain.Entities.Payments;
using SmartCareerPath.Domain.Entities.ProfileAndInterests;
using SmartCareerPath.Domain.Entities.Quiz;
using SmartCareerPath.Domain.Entities.ResumeAndParsing;
using SmartCareerPath.Domain.Entities.SkillManagement;
using SmartCareerPath.Domain.Entities.SubscriptionsAndBilling;
using SmartCareerPath.Infrastructure.Persistence.Data.Configurations.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Infrastructure.Persistence.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        #region DbSets - Auth & Users
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<AuthToken> AuthTokens { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        #endregion

        #region DbSets - Skills
        public DbSet<Skill> Skills { get; set; }
        public DbSet<UserSkill> UserSkills { get; set; }
        public DbSet<JobSkill> JobSkills { get; set; }
        public DbSet<QuizSkill> QuizSkills { get; set; }
        #endregion

        #region DbSets - Interests
        public DbSet<Interest> Interests { get; set; }
        public DbSet<UserInterest> UserInterests { get; set; }
        #endregion

        #region DbSets - Career Path
        public DbSet<CareerPath> CareerPaths { get; set; }
        public DbSet<CareerPathSkill> CareerPathSkills { get; set; }
        public DbSet<CareerPathStep> CareerPathSteps { get; set; }
        public DbSet<CareerPathTask> CareerPathTasks { get; set; }
        public DbSet<UserCareerPath> UserCareerPaths { get; set; }
        public DbSet<UserCareerPathProgress> UserCareerPathProgress { get; set; }
        public DbSet<SkillGap> SkillGaps { get; set; }
        #endregion

        #region DbSets - Quiz
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizOption> QuizOptions { get; set; }
        public DbSet<QuizSession> QuizSessions { get; set; }
        public DbSet<QuizAnswer> QuizAnswers { get; set; }
        #endregion

        #region DbSets - Resume
        public DbSet<Resume> Resumes { get; set; }
        public DbSet<ResumeParsingResult> ResumeParsingResults { get; set; }
        public DbSet<ResumeKeyword> ResumeKeywords { get; set; }
        public DbSet<ResumeSuggestion> ResumeSuggestions { get; set; }
        public DbSet<ResumeScore> ResumeScores { get; set; }
        #endregion

        #region DbSets - Job
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<ApplicationStatusHistory> ApplicationStatusHistory { get; set; }
        public DbSet<JobAnalysis> JobAnalyses { get; set; }
        #endregion

        #region DbSets - Interview
        public DbSet<InterviewSession> InterviewSessions { get; set; }
        public DbSet<InterviewQuestion> InterviewQuestions { get; set; }
        public DbSet<InterviewScore> InterviewScores { get; set; }
        public DbSet<InterviewImprovementSuggestion> InterviewImprovementSuggestions { get; set; }
        public DbSet<InterviewKeyword> InterviewKeywords { get; set; }
        #endregion

        #region DbSets - AI & Templates
        public DbSet<AIRequest> AIRequests { get; set; }
        public DbSet<AIResponseCache> AIResponseCache { get; set; }
        public DbSet<CVTemplate> CVTemplates { get; set; }
        public DbSet<CVExport> CVExports { get; set; }
        #endregion

        #region DbSets - Subscriptions & Payments
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<TokenUsage> TokenUsage { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        #endregion

        #region DbSets - Notifications & Recommendations
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserRecommendation> UserRecommendations { get; set; }
        #endregion

        #region DbSets - System
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<SystemConfig> SystemConfigs { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);

            // Additional global configurations
            ConfigureGlobalFilters(modelBuilder);
            ConfigureDecimalPrecision(modelBuilder);
            ConfigureStringMaxLengths(modelBuilder);
        }

        private void ConfigureGlobalFilters(ModelBuilder modelBuilder)
        {
            // Global query filter for soft delete
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                    var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                    var filter = System.Linq.Expressions.Expression.Lambda(
                        System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false)),
                        parameter
                    );

                    entityType.SetQueryFilter(filter);
                }
            }
        }

        private void ConfigureDecimalPrecision(ModelBuilder modelBuilder)
        {
            // Set default decimal precision for money fields
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }
        }

        private void ConfigureStringMaxLengths(ModelBuilder modelBuilder)
        {
            // Set default max length for strings without explicit length
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(string) && p.GetMaxLength() == null))
            {
                property.SetMaxLength(500);
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Auto-update timestamps
            UpdateTimestamps();

            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            // Auto-update timestamps
            UpdateTimestamps();

            return base.SaveChanges();
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        // Soft delete helper method
        public void SoftDelete<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            entity.IsDeleted = true;
            entity.UpdatedAt = DateTime.UtcNow;
            Entry(entity).State = EntityState.Modified;
        }

        // Restore soft deleted entity
        public void RestoreDeleted<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            entity.IsDeleted = false;
            entity.UpdatedAt = DateTime.UtcNow;
            Entry(entity).State = EntityState.Modified;
        }
    }
}