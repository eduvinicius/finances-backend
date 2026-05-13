using AutoMapper;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using MyFinances.App.Abstractions;
using MyFinances.App.Configuration;
using MyFinances.App.DTOs;

namespace MyFinances.App.Services
{
    public class AuthService : IAuthService
    {
        private const string InvalidCredentialsMessage = "Credenciais inválidas.";
        private const string UserNotFoundMessage = "Usuário não encontrado";

        private readonly IUserRepository _userRepo;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;
        private readonly IFileValidator _fileValidator;
        private readonly ILogger<AuthService> _logger;
        private readonly IJwtTokenGenerator _jwt;
        private readonly GoogleSettings _googleSettings;

        public AuthService(
            IUserRepository userRepo,
            IUnitOfWork uow,
            IMapper mapper,
            ILogger<AuthService> logger,
            IFileStorageService fileStorageService,
            IFileValidator fileValidator,
            IJwtTokenGenerator jwt,
            IOptions<GoogleSettings> googleSettings)
        {
            _userRepo = userRepo;
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
            _fileStorageService = fileStorageService;
            _fileValidator = fileValidator;
            _jwt = jwt;
            _googleSettings = googleSettings.Value;
        }

        public async Task<UserDto> GetUserByIdAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId) ?? throw new NotFoundException(UserNotFoundMessage);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
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

            return new AuthResponseDto
            {
                Token = _jwt.GenerateToken(user),
                Role = user.Role.ToString()
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

            var user = await _userRepo.GetByEmailAsync(dto.Email);

            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Failed login attempt for email: {Email} - User not found or inactive", dto.Email);
                throw new BadRequestException(InvalidCredentialsMessage);
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for email: {Email} - Invalid password", dto.Email);
                throw new BadRequestException(InvalidCredentialsMessage);
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("User {UserId} logged in successfully", user.Id);

            return new AuthResponseDto
            {
                Token = _jwt.GenerateToken(user),
                Role = user.Role.ToString()
            };
        }

        public async Task<GoogleAuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto)
        {
            var clientId = _googleSettings.ClientId;

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(
                    dto.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings { Audience = [clientId] }
                );
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning(ex, "Invalid Google ID token");
                throw new BadRequestException("Token do Google inválido.");
            }

            var user = await _userRepo.GetByGoogleSubjectIdAsync(payload.Subject);

            if (user != null && !user.IsActive)
                throw new BadRequestException(InvalidCredentialsMessage);

            if (user == null)
            {
                user = await _userRepo.GetByEmailAsync(payload.Email);

                if (user != null && !user.IsActive)
                    throw new BadRequestException(InvalidCredentialsMessage);

                if (user == null)
                {
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        Email = payload.Email,
                        FullName = payload.Name,
                        ProfileImageUrl = payload.Picture,
                        GoogleSubjectId = payload.Subject,
                        LastLoginAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        LastUpdatedAt = DateTime.UtcNow,
                    };
                    await _userRepo.AddAsync(user);
                }
                else
                {
                    user.GoogleSubjectId = payload.Subject;
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userRepo.UpdateAsync(user);
                }
            }
            else
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepo.UpdateAsync(user);
            }

            await _uow.SaveChangesAsync();

            var token = _jwt.GenerateToken(user);
            _logger.LogInformation("User {UserId} authenticated via Google", user.Id);

            return new GoogleAuthResponseDto
            {
                Token = token,
                Role = user.Role.ToString(),
                User = new GoogleUserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.FullName,
                    PictureUrl = user.ProfileImageUrl,
                }
            };
        }

        public async Task<string> UploadProfileImageAsync(Guid userId, IFormFile file)
        {
            _fileValidator.ValidateProfileImage(file);

            var user = await _userRepo.GetByIdAsync(userId) ?? throw new NotFoundException(UserNotFoundMessage);

            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                try
                {
                    var uri = new Uri(user.ProfileImageUrl);
                    var oldFileName = uri.Segments[^1];
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

        public async Task<UserResponseDto> UpdateUserAsync(Guid id, UpdateUserDto user)
        {
            var existingUser = await _userRepo.GetByIdAsync(id) ?? throw new NotFoundException(UserNotFoundMessage);

            _mapper.Map(user, existingUser);

            await _userRepo.UpdateAsync(existingUser);
            await _uow.SaveChangesAsync();

            return _mapper.Map<UserResponseDto>(existingUser);
        }

        public async Task<Stream> GetProfileImageAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId) ?? throw new NotFoundException(UserNotFoundMessage);

            if (string.IsNullOrEmpty(user.ProfileImageUrl))
                throw new NotFoundException("Imagem de perfil não encontrada");

            var uri = new Uri(user.ProfileImageUrl);
            var fileName = uri.Segments[^1];

            return await _fileStorageService.DownloadAsync(fileName);
        }
    }
}
