using AutoMapper;
using MyFinances.Api.DTOs;
using MyFinances.Api.DTOs.Notifications;
using MyFinances.App.DTOs;
using MyFinances.Domain.Entities;

namespace MyFinances.Api.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.LastUpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.Password)));

            CreateMap<UserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.LastUpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<User, UserDto>();

            CreateMap<TransactionDto, Transaction>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<Transaction, TransactionResponseDto>()
                .ForMember(dest => dest.Account, opt => opt.MapFrom(src =>
                    new TransactionAccountSummaryDto(src.Account.Id, src.Account.Name)))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src =>
                    new TransactionCategorySummaryDto(src.Category.Id, src.Category.Name, src.Category.Type.ToString())));

            CreateMap<CategoryDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<Category, CategoryResponseDto>();

            CreateMap<AccountDto, Account>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());

            CreateMap<Account, AccountResponseDto>();

            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));

            CreateMap<User, AdminUserListItemResponseDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.LastLogin, opt => opt.MapFrom(src => src.LastLoginAt));

            CreateMap<User, AdminUserDetailResponseDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.LastLogin, opt => opt.MapFrom(src => src.LastLoginAt));

            CreateMap<Notification, NotificationResponseDto>()
                .ForMember(dest => dest.TargetingMode, opt => opt.MapFrom(src => src.TargetingMode.ToString()))
                .ForMember(dest => dest.DeliveryChannel, opt => opt.MapFrom(src => src.DeliveryChannel.ToString()));

            CreateMap<UserNotification, UserNotificationResponseDto>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Notification.Title))
                .ForMember(dest => dest.Body, opt => opt.MapFrom(src => src.Notification.Body))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Notification.CreatedAt));
        }
    }
}

