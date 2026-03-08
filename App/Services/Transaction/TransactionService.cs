using AutoMapper;
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
                ?? throw new NotFoundException("Transação não encontrada.");

            if (transaction.UserId != userId)
                throw new ForbiddenException("Você não tem permissão para acessar esta transação.");

            var transactionMapped = _mapper.Map<TransactionResponseDto>(transaction);

            return transactionMapped;
        }

        public async Task<TransactionResponseDto> CreateAsync(TransactionDto dto)
        {
            var userId = _currentUserService.UserId;
            var account = await _accountRepo.GetByIdAsync(dto.AccountId)
             ?? throw new NotFoundException("Conta não encontrada.");

            if (account.UserId != userId)
                throw new ForbiddenException("Você não tem permissão para acessar esta conta.");

            if (!account.IsActive)
                throw new BadRequestException("Conta inativa.");

            var category = await _categoryRepo.GetByIdAsync(dto.CategoryId)
            ?? throw new NotFoundException("Categoria não encontrada.");

            if (category.UserId != userId)
                throw new ForbiddenException("Você não tem permissão para acessar esta categoria.");

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
                _logger.LogError(ex, "Erro ao criar transação");
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
