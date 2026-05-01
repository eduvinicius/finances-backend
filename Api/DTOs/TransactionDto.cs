using System.ComponentModel.DataAnnotations;
using MyFinances.Domain.Enums;

namespace MyFinances.Api.DTOs
{
    public class TransactionDto
    {
        [Required]
        public Guid AccountId { get; set; }
        [Required]
        public Guid CategoryId { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser positivo.")]
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        [Required]
        public DateTime Date { get; set; }
    }
}
