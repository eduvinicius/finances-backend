using MyFinances.Domain.Enums;

namespace MyFinances.Api.DTOs
{
    public class AccountDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public AccountType Type { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
    }
}
