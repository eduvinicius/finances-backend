using AutoMapper;
using MyFinances.Api.DTOs;
using MyFinances.App.Abstractions;
using MyFinances.App.Shared;
using MyFinances.Domain.Enums;

namespace MyFinances.App.Services.Admin
{
    public class AdminUserService(
        IUserRepository userRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper) : IAdminUserService
    {
        private const string UserNotFound = "Usuário não encontrado.";

        private readonly IUserRepository _userRepo = userRepo;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IMapper _mapper = mapper;

        public async Task<PagedResultBase<AdminUserListItemResponseDto>> GetUsersAsync(AdminUserFilterDto filters)
        {
            var adminId = _currentUserService.UserId;
            var pagedUsers = await _userRepo.GetAllFilteredAsync(filters, adminId);

            var mappedItems = _mapper.Map<IReadOnlyCollection<AdminUserListItemResponseDto>>(pagedUsers.Items);

            return new PagedResultBase<AdminUserListItemResponseDto>
            {
                Items = mappedItems,
                TotalCount = pagedUsers.TotalCount
            };
        }

        public async Task<AdminUserDetailResponseDto> GetUserByIdAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId)
                ?? throw new NotFoundException(UserNotFound);

            return _mapper.Map<AdminUserDetailResponseDto>(user);
        }

        public async Task ChangeUserRoleAsync(Guid userId, UserRole newRole)
        {
            if (!Enum.IsDefined(typeof(UserRole), newRole))
                throw new BadRequestException("Papel de usuário inválido.");

            var adminId = _currentUserService.UserId;

            if (userId == adminId)
                throw new BadRequestException("Você não pode alterar seu próprio papel.");

            var user = await _userRepo.GetByIdAsync(userId)
                ?? throw new NotFoundException(UserNotFound);

            user.Role = newRole;
            await _userRepo.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeactivateUserAsync(Guid userId)
        {
            var adminId = _currentUserService.UserId;

            if (userId == adminId)
                throw new BadRequestException("Você não pode desativar sua própria conta.");

            var user = await _userRepo.GetByIdAsync(userId)
                ?? throw new NotFoundException(UserNotFound);

            if (!user.IsActive)
                throw new BadRequestException("O usuário já está inativo.");

            user.IsActive = false;
            await _userRepo.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ActivateUserAsync(Guid userId)
        {
            var adminId = _currentUserService.UserId;

            if (userId == adminId)
                throw new BadRequestException("Você não pode ativar sua própria conta.");

            var user = await _userRepo.GetByIdAsync(userId)
                ?? throw new NotFoundException(UserNotFound);

            if (user.IsActive)
                throw new BadRequestException("O usuário já está ativo.");

            user.IsActive = true;
            await _userRepo.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            var adminId = _currentUserService.UserId;

            if (userId == adminId)
                throw new BadRequestException("Você não pode excluir sua própria conta.");

            var user = await _userRepo.GetByIdAsync(userId)
                ?? throw new NotFoundException(UserNotFound);

            _userRepo.Delete(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
