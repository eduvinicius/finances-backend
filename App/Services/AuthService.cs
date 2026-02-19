using AutoMapper;
using MyFinances.Api.DTOs;
using MyFinances.Api.Models;
using MyFinances.App.Abstractions;
using MyFinances.App.Services.Interfaces;
using MyFinances.Domain.Entities;
using MyFinances.Domain.Exceptions;
using MyFinances.Infrasctructure.Repositories.Interfaces;
using MyFinances.Infrasctructure.Security;

namespace MyFinances.App.Services
{
    public class AuthService(
        IUserRepository userRepo,
        IUnitOfWork uow,
        IMapper mapper,
        ILogger<AuthService> logger,
        IFileStorageService fileStorageService,
        JwtTokenGenerator jwt) : IAuthService
    {
        private readonly IUserRepository _userRepo = userRepo;
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;
        private readonly IFileStorageService _fileStorageService = fileStorageService;
        private readonly ILogger<AuthService> _logger = logger;
        private readonly JwtTokenGenerator _jwt = jwt;

        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepo.GetByIdAsync(id) ?? throw new NotFoundException("User not found");
            var userDto = _mapper.Map<UserDto>(user);

            return userDto;
        }

        public async Task RegisterAsync(RegisterDto dto)
        {
            _logger.LogInformation("Attempting to register user with email: {Email}", dto.Email);

            if (await _userRepo.GetByEmailAsync(dto.Email) != null)
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists", dto.Email);
                throw new ConflictException("Email already registered.");
            }

            var user = _mapper.Map<User>(dto);

            await _userRepo.AddAsync(user);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("User {UserId} registered successfully with email: {Email}", user.Id, dto.Email);
        }

        public async Task<UserResponse> LoginAsync(LoginDto dto)
        {
            _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

            var user = await _userRepo.GetByEmailAsync(dto.Email)
                ?? throw new BadRequestException("Invalid credentials.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for email: {Email} - Invalid password", dto.Email);
                throw new BadRequestException("Invalid credentials.");
            }

            var token = _jwt.GenerateToken(user);
            _logger.LogInformation("User {UserId} logged in successfully", user.Id);

            var response = new UserResponse
            { 
                FullName = user.FullName,
                NickName = user.Nickname,
                Token = token,
                ProfileImageUrl = user.ProfileImageUrl ?? ""
            };

            return response;
        }

        public async Task<string> UploadProfileImageAsync(Guid userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("Invalid file");

            if (!file.ContentType.StartsWith("image/"))
                throw new Exception("Only images allowed");

            if (file.Length > 5 * 1024 * 1024)
                throw new Exception("Max 5MB");

            var fileName = $"{userId}-{Guid.NewGuid()}";

            using var stream = file.OpenReadStream();

            var imageUrl = await _fileStorageService.UploadAsync(
                fileName,
                stream,
                file.ContentType);

            var user = await _userRepo.GetByIdAsync(userId) ?? throw new NotFoundException("User not found");
            user.ProfileImageUrl = imageUrl;

            await _userRepo.UpdateAsync(user);
            return imageUrl;
        }

        public async Task<User> UpdateUserAsync(Guid userId, UserDto user)
        {

            var existingUser = await _userRepo.GetByIdAsync(userId) ?? throw new NotFoundException("User not found");

            _mapper.Map(user, existingUser);

            await _userRepo.UpdateAsync(existingUser);

            return existingUser;
        }

    }
}
