using Buglens.Contract.IServices;
using System.Net;
using System.Net.Mail;

namespace Buglens.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string userName)
        {
            try
            {
                _logger.LogInformation($"Sending password reset email to {toEmail}");

                var frontendUrl = _configuration["Frontend:Url"];
                var resetLink = $"{frontendUrl}/reset-password.html?token={resetToken}";

                var fromEmail = _configuration["Email:FromEmail"];
                var fromPassword = _configuration["Email:FromPassword"];

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "BugLens"),
                    Subject = "Password Reset Request - BugLens",
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2>Password Reset Request</h2>
                            <p>Hello {userName},</p>
                            <p>We received a request to reset your password.</p>
                            <p><a href='{resetLink}' style='display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>
                            <p>Or copy this link: {resetLink}</p>
                            <p>This link expires in 1 hour.</p>
                            <p>Thanks,<br>BugLens Team</p>
                        </body>
                        </html>
                    ",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                using var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, fromPassword),
                    EnableSsl = true,
                    Timeout = 10000 
                };

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}");
                throw;
            }
        }
    }
}