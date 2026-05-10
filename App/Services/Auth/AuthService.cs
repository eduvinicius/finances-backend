using AutoMapper;
using Google.Apis.Auth;
using MyFinances.Api.DTOs;
using MyFinances.Infrastructure.Security;
using MyFinances.Infrastructure.Validators;
using UpdateUserDto = MyFinances.Api.DTOs.UpdateUserDto;

namespace MyFinances.App.Services
{
    public class AuthService(
        IUserRepository userRepo,
        IUnitOfWork uow,
        IMapper mapper,
        ILogger<AuthService> logger,
        IFileStorageService fileStorageService,
        IFileValidator fileValidator,
        JwtTokenGenerator jwt,
        IConfiguration config) : IAuthService
    {
        private readonly IUserRepository _userRepo = userRepo;
        private readonly IUnitOfWork _uow = uow;
        private readonly IMapper _mapper = mapper;
        private readonly IFileStorageService _fileStorageService = fileStorageService;
        private readonly IFileValidator _fileValidator = fileValidator;
        private readonly ILogger<AuthService> _logger = logger;
        private readonly JwtTokenGenerator _jwt = jwt;
        private readonly IConfiguration _config = config;

        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepo.GetByIdAsync(id) ?? throw new NotFoundException("Usu�rio n�o encontrado");
            var userDto = _mapper.Map<UserDto>(user);

            return userDto;
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

            var user = await _userRepo.GetByEmailAsync(dto.Email)
                ?? throw new BadRequestException("Credenciais inv�lidas.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login attempt for email: {Email} - Invalid password", dto.Email);
                throw new BadRequestException("Credenciais inv�lidas.");
            }

            if (!user.IsActive)
                throw new BadRequestException("Conta desativada. Entre em contato com o administrador.");

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
            var clientId = _config["Google:ClientId"]
                ?? throw new InvalidOperationException("Google ClientId is not configured.");

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

            if (user == null)
            {
                user = await _userRepo.GetByEmailAsync(payload.Email);

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

            var user = await _userRepo.GetByIdAsync(userId) ?? throw new NotFoundException("Usu�rio n�o encontrado");

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

            var existingUser = await _userRepo.GetByIdAsync(userId) ?? throw new NotFoundException("Usu�rio n�o encontrado");

            _mapper.Map(user, existingUser);

            await _userRepo.UpdateAsync(existingUser);
            await _uow.SaveChangesAsync();

            return _mapper.Map<UserResponseDto>(existingUser);
        }

        public async Task<Stream> GetProfileImageAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId) ?? throw new NotFoundException("Usu�rio n�o encontrado");

            if (string.IsNullOrEmpty(user.ProfileImageUrl))
                throw new NotFoundException("Imagem de perfil n�o encontrada");

            var uri = new Uri(user.ProfileImageUrl);
            var fileName = uri.Segments.Last();

            return await _fileStorageService.DownloadAsync(fileName);
        }

    }
}
