namespace MyFinances.App.Services.PasswordReset
{
    public interface IPasswordResetService
    {
        Task RequestResetAsync(string email);
        Task ResetPasswordAsync(string email, string rawToken, string newPassword);
    }
}
