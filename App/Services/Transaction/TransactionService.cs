using AutoMapper;
using ClosedXML.Excel;
using MyFinances.Api.DTOs;
using MyFinances.App.Filters;
using MyFinances.App.Shared;
using MyFinances.Domain.Enums;

namespace MyFinances.App.Services
{
    public class TransactionService(
        IAccountRepository accountRepo,
        ITransactionRepository transactionRepo,
        ICategoryRepository categoryRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<TransactionService> logger
        ) : ITransactionService
    {
        private readonly IAccountRepository _accountRepo = accountRepo;
        private readonly ITransactionRepository _transactionRepo = transactionRepo;
        private readonly ICategoryRepository _categoryRepo = categoryRepo;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<TransactionService> _logger = logger;

        public async Task<PagedResultBase<TransactionResponseDto>> GetAllByUserId(TransactionFilters filters)
        {
            var userId = _currentUserService.UserId;
            var transactions = await _transactionRepo.GetAllTransactionsAsync(userId, filters);
            var transactionsMapped = _mapper.Map<IEnumerable<TransactionResponseDto>>(transactions.Items);

            var transactionResponse = new PagedResultBase<TransactionResponseDto>
            {
                Items = (IReadOnlyCollection<TransactionResponseDto>)transactionsMapped,
                TotalCount = transactions.TotalCount,
            };

            return transactionResponse;
        }

        public async Task<TransactionResponseDto> GetByIdAsync(Guid transactionId)
        {
            var userId = _currentUserService.UserId;
            var transaction = await _transactionRepo.GetByIdAsync(transactionId)
                ?? throw new NotFoundException("Transa��o n�o encontrada.");

            if (transaction.UserId != userId)
                throw new ForbiddenException("Voc� n�o tem permiss�o para acessar esta transa��o.");

            var transactionMapped = _mapper.Map<TransactionResponseDto>(transaction);

            return transactionMapped;
        }

        public async Task<TransactionResponseDto> CreateAsync(TransactionDto dto)
        {
            var userId = _currentUserService.UserId;
            var account = await _accountRepo.GetByIdAsync(dto.AccountId)
             ?? throw new NotFoundException("Conta n�o encontrada.");

            if (account.UserId != userId)
                throw new ForbiddenException("Voc� n�o tem permiss�o para acessar esta conta.");

            if (!account.IsActive)
                throw new BadRequestException("Conta inativa.");

            var category = await _categoryRepo.GetByIdAsync(dto.CategoryId)
            ?? throw new NotFoundException("Categoria n�o encontrada.");

            if (category.UserId != userId)
                throw new ForbiddenException("Voc� n�o tem permiss�o para acessar esta categoria.");

            dto.Amount = ValidateAmount(dto.Amount, category.Type);

            ValidateBalance(account, dto.Amount);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var transaction = _mapper.Map<Transaction>(dto);
                transaction.UserId = userId;

                await _transactionRepo.AddAsync(transaction);

                account.Balance += dto.Amount;
                await _accountRepo.UpdateAsync(account);

                await _unitOfWork.CommitAsync();
                return _mapper.Map<TransactionResponseDto>(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar transa��o");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        private static decimal ValidateAmount(decimal amount, TransactionType type)
        {
            if (type == TransactionType.Expense && amount > 0)
            {
                var negativeAmount = -amount;
                return negativeAmount;
            }

           if ((type == TransactionType.Income || type == TransactionType.Investment) && amount < 0)
            {
                throw new BadRequestException("O valor deve ser positivo para receitas e investimentos.");
            }

            return amount;
        }

        public async Task<byte[]> ExportToExcelAsync(TransactionExportDto filters)
        {
            var userId = _currentUserService.UserId;
            var transactions = await _transactionRepo.GetForExportAsync(userId, filters);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Transactions");

            // Header row — order matches the frontend Transactions table
            var headers = new[] { "Conta", "Tipo de Transação", "Descrição", "Valor", "Data", "Categoria" };
            for (int col = 1; col <= headers.Length; col++)
            {
                var cell = worksheet.Cell(1, col);
                cell.Value = headers[col - 1];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.PatternType = XLFillPatternValues.Solid;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2D6A4F");
                cell.Style.Font.FontColor = XLColor.Black;
            }

            // Data rows — column order: Conta, Tipo de Transação, Descrição, Valor, Data, Categoria
            int row = 2;
            foreach (var t in transactions)
            {
                worksheet.Cell(row, 1).Value = t.Account.Name;
                worksheet.Cell(row, 2).Value = t.Type switch
                {
                    TransactionType.Income => "Receita",
                    TransactionType.Expense => "Despesa",
                    TransactionType.Investment => "Investimento",
                    _ => t.Type.ToString()
                };
                worksheet.Cell(row, 3).Value = t.Description;
                worksheet.Cell(row, 4).Value = t.Amount;
                worksheet.Cell(row, 5).Value = t.CreatedAt.Date;
                worksheet.Cell(row, 6).Value = t.Category.Name;
                row++;
            }

            // Column formatting
            var amountCol = worksheet.Column(4);
            amountCol.Style.NumberFormat.Format = "#,##0.00";

            var dateCol = worksheet.Column(5);
            dateCol.Style.NumberFormat.Format = "yyyy-MM-dd";

            // Auto-fit all columns
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static void ValidateBalance(Account account, decimal amount)
        {
            if (amount >= 0)
                return;

            if (account.Type == AccountType.Credit)
                return;

            if (account.Balance + amount < 0)
            {
                throw new BadRequestException("Saldo insuficiente na conta.");
            }
        }
    }
}
