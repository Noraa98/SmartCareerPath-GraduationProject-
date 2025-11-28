using SmartCareerPath.Domain.Common.BaseEntities;
using SmartCareerPath.Domain.Entities.AIEngine;
using SmartCareerPath.Domain.Entities.CareerPath;
using SmartCareerPath.Domain.Entities.InterviewSystem;
using SmartCareerPath.Domain.Entities.JobPostingAndMatching;
using SmartCareerPath.Domain.Entities.NotificationsLogsAndConfig;
using SmartCareerPath.Domain.Entities.Payments;
using SmartCareerPath.Domain.Entities.ProfileAndInterests;
using SmartCareerPath.Domain.Entities.Quiz;
using SmartCareerPath.Domain.Entities.ResumeAndParsing;
using SmartCareerPath.Domain.Entities.SkillManagement;
using SmartCareerPath.Domain.Entities.SubscriptionsAndBilling;
using System.ComponentModel.DataAnnotations;

namespace SmartCareerPath.Domain.Entities.Auth
{
    public class User : BaseEntity
    {
        [Required, MaxLength(256), EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string PasswordSalt { get; set; }

        [MaxLength(150)]
        public string FullName { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        public int? RoleId { get; set; }
        public Role Role { get; set; }

        // Email verification
        public bool IsEmailVerified { get; set; }
        public string EmailVerificationToken { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }

        // Account status
        public bool IsActive { get; set; } = true;
        public bool IsLocked { get; set; }
        public DateTime? LockedUntil { get; set; }
        public int FailedLoginAttempts { get; set; }

        public DateTime? LastActivity { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Navigation Properties
        public UserProfile Profile { get; set; }
        public ICollection<UserSkill> Skills { get; set; } = new List<UserSkill>();
        public ICollection<UserCareerPath> CareerPaths { get; set; } = new List<UserCareerPath>();
        public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
        public ICollection<QuizSession> QuizSessions { get; set; } = new List<QuizSession>();
        public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
        public ICollection<InterviewSession> InterviewSessions { get; set; } = new List<InterviewSession>();
        public ICollection<AIRequest> AIRequests { get; set; } = new List<AIRequest>();
        public ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
        public ICollection<AuthToken> AuthTokens { get; set; } = new List<AuthToken>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<UserRecommendation> UserRecommendations { get; set; } = new List<UserRecommendation>();
        public ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();

        // public virtual UserSubscription? UserSubscription { get; set; }
        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

    }


}
