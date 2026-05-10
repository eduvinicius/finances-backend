using System.Security.Claims;
using MyFinances.Domain.Enums;

namespace MyFinances.App.Services
{
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public Guid UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                if (string.IsNullOrEmpty(userIdClaim))
                    throw new UnauthorizedAccessException();

                return Guid.Parse(userIdClaim);
            }
        }

        public UserRole Role
        {
            get
            {
                var roleClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(ClaimTypes.Role)?
                    .Value;

                if (string.IsNullOrEmpty(roleClaim))
                    return UserRole.User;

                return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.User;
            }
        }
    }
}
