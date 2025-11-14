using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Contracts;
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
using SmartCareerPath.Infrastructure.Persistence.Data;
using SmartCareerPath.Infrastructure.Persistence.Repositories;

namespace SmartCareerPath.Infrastructure.Persistence.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly Dictionary<Type, object> _repositories;
        private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction _transaction;

        // Specific repositories
        private IRepository<User> _users;
        private IRepository<Role> _roles;
        private IRepository<UserProfile> _userProfiles;
        private IRepository<Skill> _skills;
        private IRepository<UserSkill> _userSkills;
        private IRepository<CareerPath> _careerPaths;
        private IRepository<UserCareerPath> _userCareerPaths;
        private IRepository<Quiz> _quizzes;
        private IRepository<QuizSession> _quizSessions;
        private IRepository<Resume> _resumes;
        private IRepository<JobPosting> _jobPostings;
        private IRepository<JobApplication> _jobApplications;
        private IRepository<InterviewSession> _interviewSessions;
        private IRepository<SubscriptionPlan> _subscriptionPlans;
        private IRepository<UserSubscription> _userSubscriptions;
        private IRepository<Notification> _notifications;
        private IRepository<AIRequest> _aiRequests;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            _repositories = new Dictionary<Type, object>();
        }

        public IRepository<User> Users => _users ??= new Repository<User>(_context);
        public IRepository<Role> Roles => _roles ??= new Repository<Role>(_context);
        public IRepository<UserProfile> UserProfiles => _userProfiles ??= new Repository<UserProfile>(_context);
        public IRepository<Skill> Skills => _skills ??= new Repository<Skill>(_context);
        public IRepository<UserSkill> UserSkills => _userSkills ??= new Repository<UserSkill>(_context);
        public IRepository<CareerPath> CareerPaths => _careerPaths ??= new Repository<CareerPath>(_context);
        public IRepository<UserCareerPath> UserCareerPaths => _userCareerPaths ??= new Repository<UserCareerPath>(_context);
        public IRepository<Quiz> Quizzes => _quizzes ??= new Repository<Quiz>(_context);
        public IRepository<QuizSession> QuizSessions => _quizSessions ??= new Repository<QuizSession>(_context);
        public IRepository<Resume> Resumes => _resumes ??= new Repository<Resume>(_context);
        public IRepository<JobPosting> JobPostings => _jobPostings ??= new Repository<JobPosting>(_context);
        public IRepository<JobApplication> JobApplications => _jobApplications ??= new Repository<JobApplication>(_context);
        public IRepository<InterviewSession> InterviewSessions => _interviewSessions ??= new Repository<InterviewSession>(_context);
        public IRepository<SubscriptionPlan> SubscriptionPlans => _subscriptionPlans ??= new Repository<SubscriptionPlan>(_context);
        public IRepository<UserSubscription> UserSubscriptions => _userSubscriptions ??= new Repository<UserSubscription>(_context);
        public IRepository<Notification> Notifications => _notifications ??= new Repository<Notification>(_context);
        public IRepository<AIRequest> AIRequests => _aiRequests ??= new Repository<AIRequest>(_context);

        public IRepository<T> Repository<T>() where T : BaseEntity
        {
            var type = typeof(T);

            if (!_repositories.ContainsKey(type))
            {
                _repositories[type] = new Repository<T>(_context);
            }

            return (IRepository<T>)_repositories[type];
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }
}
