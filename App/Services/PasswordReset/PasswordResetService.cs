using MyFinances.App.Abstractions;
using MyFinances.Domain.Entities;
using MyFinances.Domain.Exceptions;
using System.Security.Cryptography;

namespace MyFinances.App.Services.PasswordReset
{
    public class PasswordResetService(
        IUserRepository userRepo,
        IPasswordResetTokenRepository tokenRepo,
        IEmailService emailService,
        IUnitOfWork uow,
        IConfiguration config,
        ILogger<PasswordResetService> logger) : IPasswordResetService
    {
        private readonly IUserRepository _userRepo = userRepo;
        private readonly IPasswordResetTokenRepository _tokenRepo = tokenRepo;
        private readonly IEmailService _emailService = emailService;
        private readonly IUnitOfWork _uow = uow;
        private readonly IConfiguration _config = config;
        private readonly ILogger<PasswordResetService> _logger = logger;

        public async Task RequestResetAsync(string email)
        {
            var user = await _userRepo.GetByEmailAsync(email.ToLowerInvariant());

            // Silent return — never reveal whether the email exists or uses Google
            if (user == null || (user.GoogleSubjectId != null && user.PasswordHash == null))
            {
                _logger.LogInformation("Password reset for {Email} silently ignored (not found or Google-only)", email);
                return;
            }

            // Invalidate existing unused tokens for this user
            var existingTokens = await _tokenRepo.GetValidTokensByUserIdAsync(user.Id);
            foreach (var t in existingTokens)
                t.Used = true;

            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var resetToken = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = BCrypt.Net.BCrypt.HashPassword(rawToken),
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                Used = false,
                CreatedAt = DateTime.UtcNow,
            };

            await _tokenRepo.AddAsync(resetToken);

            var frontendBaseUrl = _config["App:FrontendBaseUrl"]
                ?? throw new InvalidOperationException("App:FrontendBaseUrl is not configured.");
            var resetLink = $"{frontendBaseUrl}/reset-password" +
                            $"?token={Uri.EscapeDataString(rawToken)}" +
                            $"&email={Uri.EscapeDataString(user.Email)}";

            await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, resetLink);

            await _uow.SaveChangesAsync();

            _logger.LogInformation("Password reset email sent to user {UserId}", user.Id);
        }

        public async Task ResetPasswordAsync(string email, string rawToken, string newPassword)
        {
            var user = await _userRepo.GetByEmailAsync(email.ToLowerInvariant())
                ?? throw new NotFoundException("Usuário não encontrado.");

            var validTokens = await _tokenRepo.GetValidTokensByUserIdAsync(user.Id);
            var token = validTokens
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefault(t => BCrypt.Net.BCrypt.Verify(rawToken, t.TokenHash));

            if (token == null)
                throw new BadRequestException("Token inválido ou expirado.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.LastUpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user);

            token.Used = true;
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Password reset successfully for user {UserId}", user.Id);
        }
    }
}
