using System.ComponentModel.DataAnnotations;
using MyFinances.Domain.Enums;

namespace MyFinances.Api.DTOs
{
    public class CategoryDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}
