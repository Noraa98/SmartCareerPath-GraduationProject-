using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Contracts.Repositories;
using SmartCareerPath.Domain.Entities.AIEngine;
using SmartCareerPath.Domain.Entities.Auth;
using SmartCareerPath.Domain.Entities.CareerPath;
using SmartCareerPath.Domain.Entities.InterviewSystem;
using SmartCareerPath.Domain.Entities.JobPostingAndMatching;
using SmartCareerPath.Domain.Entities.NotificationsLogsAndConfig;
using SmartCareerPath.Domain.Entities.ProfileAndInterests;
using SmartCareerPath.Domain.Entities.Quiz;
using SmartCareerPath.Domain.Entities.ResumeAndParsing;
using SmartCareerPath.Domain.Entities.SkillManagement;
using SmartCareerPath.Domain.Entities.SubscriptionsAndBilling;

namespace SmartCareerPath.Domain.Contracts
{
    public interface IUnitOfWork : IDisposable
    {
        // Repositories
        IRepository<User> Users { get; }
        IRepository<Role> Roles { get; }
        IRepository<UserProfile> UserProfiles { get; }
        IRepository<Skill> Skills { get; }
        IRepository<UserSkill> UserSkills { get; }
        IRepository<CareerPath> CareerPaths { get; }
        IRepository<UserCareerPath> UserCareerPaths { get; }
        IRepository<Quiz> Quizzes { get; }
        IRepository<QuizSession> QuizSessions { get; }
        IRepository<Resume> Resumes { get; }
        IRepository<JobPosting> JobPostings { get; }
        IRepository<JobApplication> JobApplications { get; }
        IRepository<InterviewSession> InterviewSessions { get; }
        IRepository<SubscriptionPlan> SubscriptionPlans { get; }
        IRepository<UserSubscription> UserSubscriptions { get; }
        IRepository<Notification> Notifications { get; }
        IRepository<AIRequest> AIRequests { get; }

        // Generic repository for any entity
        IRepository<T> Repository<T>() where T : BaseEntity;

        // Save changes
        Task<int> SaveChangesAsync();
        int SaveChanges();

        // Transaction support
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
