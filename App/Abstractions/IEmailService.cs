namespace MyFinances.App.Abstractions
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink);
        Task SendEmailAsync(string toEmail, string toName, string subject, string body);
    }
}
