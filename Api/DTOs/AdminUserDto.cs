using MyFinances.App.Filters;
using MyFinances.Domain.Enums;

namespace MyFinances.Api.DTOs
{
    public class AdminUserListItemResponseDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Nickname { get; set; }
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    public class AdminUserDetailResponseDto : AdminUserListItemResponseDto
    {
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? DocumentNumber { get; set; }
    }

    public class AdminUserFilterDto : PaginationFilterBase
    {
        public string? FullName { get; set; }
        public string? Nickname { get; set; }
        public string? DocumentNumber { get; set; }
        public UserRole? Role { get; set; }
        public DateTime? CreatedAtFrom { get; set; }
        public DateTime? CreatedAtTo { get; set; }
    }

    public class ChangeUserRoleDto
    {
        public UserRole Role { get; set; }
    }
}
