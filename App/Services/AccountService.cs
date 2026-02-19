using AutoMapper;
using MyFinances.Api.DTOs;
using MyFinances.App.Filters;
using MyFinances.App.Services.Interfaces;
using MyFinances.App.Shared;
using MyFinances.Domain.Entities;
using MyFinances.Domain.Enums;
using MyFinances.Domain.Exceptions;
using MyFinances.Infrasctructure.Repositories.Interfaces;

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
        public async Task<IEnumerable<Account>> GetAllAsync()
        {
            var userId = _currentUserService.UserId;
            return await _accountRepo.GetAllByUserIdAsync(userId);
        }

        public async Task<PagedResultBase<Account>> GetPaginatedAsync(AccountFilters filters)
        {
            var userId = _currentUserService.UserId;
            return await _accountRepo.GetPaginatedByUserIdAsync(userId, filters);
        }

        public async Task<Account> GetByIdAsync(Guid id)
        {
            var account = await _accountRepo.GetByIdAsync(id)
                ?? throw new NotFoundException("Account not found.");

            if (account.UserId != _currentUserService.UserId)
                throw new ForbiddenException("You do not have access to this account.");

            return account;
        }

        public async Task<Account> CreateAsync(AccountDto dto)
        {
            if (dto.Balance < 0 && dto.Type != AccountType.Credit)
                throw new BadRequestException("Only credit accounts may start with negative balance.");

            var account = _mapper.Map<Account>(dto);
            account.UserId = _currentUserService.UserId;
            account.IsActive = true;

            await _accountRepo.AddAsync(account);
            await _unitOfWork.SaveChangesAsync();

            return account;
        }

        public async Task DeactivateAsync(Guid id)
        {
            var account = await GetByIdAsync(id);

            account.IsActive = false;
            await _accountRepo.UpdateAsync(account);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
