using MyFinances.Domain.Enums;

namespace MyFinances.App.Services
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        UserRole Role { get; }
    }
}
