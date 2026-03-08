using AutoMapper;
using MyFinances.Api.DTOs;
using MyFinances.Infrastructure.Security;
using UpdateUserDto = MyFinances.Api.DTOs.UpdateUserDto;

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
            var user = await _userRepo.GetByIdAsync(id) ?? throw new NotFoundException("Usuário não encontrado");
            var userDto = _mapper.Map<UserDto>(user);

            return userDto;
        }

        public async Task RegisterAsync(RegisterDto dto)
        {
            _logger.LogInformation("Attempting to register user with email: {Email}", dto.Email);

            if (await _userRepo.GetByEmailAsync(dto.Email) != null)
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists", dto.Email);
                throw new ConflictException("Email já cadastrado.");
            }

            var user = _mapper.Map<User>(dto);

            await _userRepo.AddAsync(user);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("User {UserId} registered successfully with email: {Email}", user.Id, dto.Email);
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

            var user = await _userRepo.GetByEmailAsync(dto.Email)
                ?? throw new BadRequestException("Credenciais inválidas.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for email: {Email} - Invalid password", dto.Email);
                throw new BadRequestException("Credenciais inválidas.");
            }

            var token = _jwt.GenerateToken(user);
            _logger.LogInformation("User {UserId} logged in successfully", user.Id);

            return token;
        }

        public async Task<string> UploadProfileImageAsync(Guid userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("Arquivo inválido");

            if (!file.ContentType.StartsWith("image/"))
                throw new Exception("Apenas imagens são permitidas");

            if (file.Length > 5 * 1024 * 1024)
                throw new Exception("Máximo de 5MB");

            var user = await _userRepo.GetByIdAsync(userId) ?? throw new NotFoundException("Usuário não encontrado");

            // Delete old profile image if it exists
            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                try
                {
                    var uri = new Uri(user.ProfileImageUrl);
                    var oldFileName = uri.Segments.Last();
                    await _fileStorageService.DeleteAsync(oldFileName);
                    _logger.LogInformation("Old profile image deleted for user {UserId}", userId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete old profile image for user {UserId}", userId);
                }
            }

            var fileName = $"{userId}-{Guid.NewGuid()}";

            using var stream = file.OpenReadStream();

            var imageUrl = await _fileStorageService.UploadAsync(
                fileName,
                stream,
                file.ContentType);

            user.ProfileImageUrl = imageUrl;

            await _userRepo.UpdateAsync(user);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Profile image uploaded successfully for user {UserId}", userId);

            return imageUrl;
        }

        public async Task<UserResponseDto> UpdateUserAsync(Guid userId, UpdateUserDto user)
        {

            var existingUser = await _userRepo.GetByIdAsync(userId) ?? throw new NotFoundException("Usuário não encontrado");

            _mapper.Map(user, existingUser);

            await _userRepo.UpdateAsync(existingUser);
            await _uow.SaveChangesAsync();

            return _mapper.Map<UserResponseDto>(existingUser);
        }

        public async Task<Stream> GetProfileImageAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId) ?? throw new NotFoundException("Usuário não encontrado");

            if (string.IsNullOrEmpty(user.ProfileImageUrl))
                throw new NotFoundException("Imagem de perfil não encontrada");

            // Extract filename from the full URL
            // URL format: https://mzzvhtvojiqbvhitkcbw.supabase.co/storage/v1/object/public/ProfileImage/{fileName}
            var uri = new Uri(user.ProfileImageUrl);
            var fileName = uri.Segments.Last();

            return await _fileStorageService.DownloadAsync(fileName);
        }

    }
}
