using AutoMapper;
using MyFinances.App.DTOs;
using MyFinances.App.Filters;
using MyFinances.App.Shared;
using MyFinances.Domain.Entities;
using MyFinances.Domain.Enums;
using MyFinances.Domain.Exceptions;
using MyFinances.Infrastructure.Repositories.Interfaces;

namespace MyFinances.App.Services
{
    public class AccountService(
        IAccountRepository accountRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper
        ) : IAccountService
    {
        private readonly IAccountRepository _accountRepo = accountRepo;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IMapper _mapper = mapper;
        
        public async Task<IEnumerable<AccountResponseDto>> GetAllAsync()
        {
            var userId = _currentUserService.UserId;
            var accounts = await _accountRepo.GetAllByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<AccountResponseDto>>(accounts);
        }

        public async Task<PagedResultBase<AccountResponseDto>> GetPaginatedAsync(AccountFilters filters)
        {
            var userId = _currentUserService.UserId;
            var result = await _accountRepo.GetPaginatedByUserIdAsync(userId, filters);
            var mappedAccounts = _mapper.Map<List<AccountResponseDto>>(result.Items);

            return new PagedResultBase<AccountResponseDto>
            {
                Items = mappedAccounts,
                TotalCount = result.TotalCount
            };
        }

        public async Task<AccountResponseDto> GetByIdAsync(Guid id)
        {
            var account = await _accountRepo.GetByIdAsync(id)
                ?? throw new NotFoundException("Conta n�o encontrada.");

            if (account.UserId != _currentUserService.UserId)
                throw new ForbiddenException("Voc� n�o tem acesso a esta conta.");

            return _mapper.Map<AccountResponseDto>(account);
        }

        public async Task<AccountResponseDto> CreateAsync(AccountDto dto)
        {
            if (dto.Balance < 0 && dto.Type != AccountType.Credit)
                throw new BadRequestException("Apenas contas de cr�dito podem come�ar com saldo negativo.");

            var account = _mapper.Map<Account>(dto);
            account.UserId = _currentUserService.UserId;
            account.IsActive = true;

            await _accountRepo.AddAsync(account);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AccountResponseDto>(account);
        }

        public async Task DeactivateAsync(Guid id)
        {
            var account = await _accountRepo.GetByIdAsync(id)
                ?? throw new NotFoundException("Conta n�o encontrada.");

            if (account.UserId != _currentUserService.UserId)
                throw new ForbiddenException("Voc� n�o tem acesso a esta conta.");

            account.IsActive = false;
            await _accountRepo.UpdateAsync(account);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
