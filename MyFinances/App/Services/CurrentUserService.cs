using System.Security.Claims;
using MyFinances.App.Services.Interfaces;

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
    }
}
