using MyFinances.Domain.Entities;

namespace MyFinances.App.Abstractions
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}
