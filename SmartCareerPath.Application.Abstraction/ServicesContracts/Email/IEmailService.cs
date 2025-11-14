using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.Abstraction.ServicesContracts.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendWelcomeEmailAsync(string to, string userName, string verificationToken);
        Task SendEmailVerificationAsync(string to, string userName, string verificationToken);
        Task SendPasswordResetEmailAsync(string to, string userName, string resetToken);
        Task SendPasswordChangedNotificationAsync(string to, string userName);
        Task SendJobApplicationConfirmationAsync(string to, string userName, string jobTitle);
        Task SendQuizResultEmailAsync(string to, string userName, string quizTitle, int score, int totalScore);
    }
}
