namespace Buglens.Contract.IServices;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string userName);
}