using MyFinances.Api.DTOs;
using MyFinances.App.DTOs;
using MyFinances.App.Filters;
using MyFinances.App.Shared;

namespace MyFinances.App.Services
{
    public interface ITransactionService
    {
        Task<PagedResultBase<TransactionResponseDto>> GetAllByUserId(TransactionFilters filters);
        Task<TransactionResponseDto> GetByIdAsync(Guid transactionId);
        Task<TransactionResponseDto> CreateAsync(TransactionDto dto);
        Task<byte[]> ExportToExcelAsync(TransactionExportDto filters);
    }
}
