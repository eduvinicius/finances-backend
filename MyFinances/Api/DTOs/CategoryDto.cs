using MyFinances.Domain.Enums;

namespace MyFinances.Api.DTOs
{
    public class CategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
