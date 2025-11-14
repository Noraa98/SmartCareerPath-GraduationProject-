using Microsoft.Extensions.Configuration;
using SmartCareerPath.Application.Abstraction.DTOs;
using SmartCareerPath.Application.Abstraction.ServicesContracts.Email;
using System.Net;
using System.Net.Mail;

namespace SmartCareerPath.Application.ServicesImplementation.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _emailSettings = new EmailSettings
            {
                SmtpServer = configuration["Email:SmtpServer"],
                SmtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587"),
                SenderEmail = configuration["Email:SenderEmail"],
                SenderName = configuration["Email:SenderName"] ?? "Smart Career Path",
                Username = configuration["Email:Username"],
                Password = configuration["Email:Password"],
                EnableSsl = bool.Parse(configuration["Email:EnableSsl"] ?? "true"),
                FrontendUrl = configuration["Email:FrontendUrl"] ?? "http://localhost:4200"
            };
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                using var message = new MailMessage();
                message.From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName);
                message.To.Add(new MailAddress(to));
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;

                using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                smtpClient.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                smtpClient.EnableSsl = _emailSettings.EnableSsl;

                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Email sending failed: {ex.Message}");
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(string to, string userName, string verificationToken)
        {
            var subject = "Welcome to Smart Career Path! 🎉";
            var verificationUrl = $"{_emailSettings.FrontendUrl}/verify-email?token={verificationToken}&email={to}";

            var body = GetWelcomeEmailTemplate(userName, verificationUrl);

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendEmailVerificationAsync(string to, string userName, string verificationToken)
        {
            var subject = "Verify Your Email Address";
            var verificationUrl = $"{_emailSettings.FrontendUrl}/verify-email?token={verificationToken}&email={to}";

            var body = GetEmailVerificationTemplate(userName, verificationUrl);

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string to, string userName, string resetToken)
        {
            var subject = "Reset Your Password";
            var resetUrl = $"{_emailSettings.FrontendUrl}/reset-password?token={resetToken}&email={to}";

            var body = GetPasswordResetTemplate(userName, resetUrl);

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPasswordChangedNotificationAsync(string to, string userName)
        {
            var subject = "Your Password Has Been Changed";
            var body = GetPasswordChangedTemplate(userName);

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendJobApplicationConfirmationAsync(string to, string userName, string jobTitle)
        {
            var subject = $"Application Received for {jobTitle}";
            var body = GetJobApplicationTemplate(userName, jobTitle);

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendQuizResultEmailAsync(string to, string userName, string quizTitle, int score, int totalScore)
        {
            var subject = $"Your {quizTitle} Results";
            var percentage = (int)((double)score / totalScore * 100);
            var body = GetQuizResultTemplate(userName, quizTitle, score, totalScore, percentage);

            await SendEmailAsync(to, subject, body);
        }

        #region Email Templates

        private string GetWelcomeEmailTemplate(string userName, string verificationUrl)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; padding: 12px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; font-size: 12px; color: #777; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Welcome to Smart Career Path!</h1>
        </div>
        <div class='content'>
            <h2>Hi {userName}!</h2>
            <p>We're thrilled to have you join our community! Smart Career Path is here to help you achieve your career goals.</p>
            
            <p><strong>Here's what you can do:</strong></p>
            <ul>
                <li>✅ Build and optimize your resume with AI</li>
                <li>✅ Take skill assessment quizzes</li>
                <li>✅ Get personalized career path recommendations</li>
                <li>✅ Apply to jobs that match your skills</li>
                <li>✅ Practice with mock interviews</li>
            </ul>

            <p>First, let's verify your email address:</p>
            <center>
                <a href='{verificationUrl}' class='button'>Verify Email Address</a>
            </center>

            <p>If the button doesn't work, copy and paste this link into your browser:</p>
            <p style='word-break: break-all; font-size: 12px;'>{verificationUrl}</p>

            <p>Ready to start your career journey? Log in and explore!</p>
        </div>
        <div class='footer'>
            <p>© 2024 Smart Career Path. All rights reserved.</p>
            <p>If you didn't create this account, please ignore this email.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetEmailVerificationTemplate(string userName, string verificationUrl)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #667eea; color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; padding: 12px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📧 Verify Your Email</h1>
        </div>
        <div class='content'>
            <h2>Hi {userName}!</h2>
            <p>Please verify your email address by clicking the button below:</p>
            
            <center>
                <a href='{verificationUrl}' class='button'>Verify Email</a>
            </center>

            <p>This link will expire in 24 hours.</p>
            
            <p>If you didn't request this verification, please ignore this email.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetPasswordResetTemplate(string userName, string resetUrl)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #f56565; color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; padding: 12px 30px; background: #f56565; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .warning {{ background: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Reset Your Password</h1>
        </div>
        <div class='content'>
            <h2>Hi {userName}!</h2>
            <p>We received a request to reset your password. Click the button below to create a new password:</p>
            
            <center>
                <a href='{resetUrl}' class='button'>Reset Password</a>
            </center>

            <div class='warning'>
                <strong>⚠️ Security Notice:</strong> This link will expire in 2 hours for your security.
            </div>

            <p>If you didn't request a password reset, please ignore this email and your password will remain unchanged.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetPasswordChangedTemplate(string userName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #48bb78; color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ Password Changed Successfully</h1>
        </div>
        <div class='content'>
            <h2>Hi {userName}!</h2>
            <p>Your password has been changed successfully.</p>
            
            <p>If you didn't make this change, please contact our support team immediately.</p>
            
            <p><strong>When:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetJobApplicationTemplate(string userName, string jobTitle)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #4299e1; color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📝 Application Received!</h1>
        </div>
        <div class='content'>
            <h2>Hi {userName}!</h2>
            <p>We've received your application for the <strong>{jobTitle}</strong> position.</p>
            
            <p><strong>What happens next?</strong></p>
            <ul>
                <li>The employer will review your application</li>
                <li>You'll be notified if you're shortlisted</li>
                <li>Check your dashboard for updates</li>
            </ul>

            <p>Good luck! 🍀</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetQuizResultTemplate(string userName, string quizTitle, int score, int totalScore, int percentage)
        {
            var emoji = percentage >= 80 ? "🎉" : percentage >= 60 ? "👍" : "💪";
            var message = percentage >= 80 ? "Excellent work!" : percentage >= 60 ? "Good job!" : "Keep practicing!";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #9f7aea; color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .score-box {{ background: white; padding: 20px; border-radius: 10px; text-align: center; margin: 20px 0; border: 2px solid #9f7aea; }}
        .score {{ font-size: 48px; font-weight: bold; color: #9f7aea; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{emoji} Quiz Results</h1>
        </div>
        <div class='content'>
            <h2>Hi {userName}!</h2>
            <p>You've completed the <strong>{quizTitle}</strong> quiz. {message}</p>
            
            <div class='score-box'>
                <div class='score'>{percentage}%</div>
                <p>You scored <strong>{score} out of {totalScore}</strong></p>
            </div>

            <p>View your detailed results and recommendations in your dashboard.</p>
        </div>
    </div>
</body>
</html>";
        }

        #endregion
    }


}

