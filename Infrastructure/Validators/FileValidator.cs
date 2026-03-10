using Microsoft.Extensions.Options;
using MyFinances.Domain.Exceptions;
using MyFinances.Infrastructure.Configuration;

namespace MyFinances.Infrastructure.Validators
{
    public interface IFileValidator
    {
        void ValidateProfileImage(IFormFile file);
    }

    public class FileValidator(IOptions<ApiSettings> options) : IFileValidator
    {
        private readonly ApiSettings _settings = options.Value;

        public void ValidateProfileImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ValidationException("Arquivo inválido");

            if (!_settings.AllowedImageContentTypes.Contains(file.ContentType))
                throw new ValidationException("Apenas imagens são permitidas");

            var maxSize = _settings.MaxProfileImageSizeInMb * 1024 * 1024;
            if (file.Length > maxSize)
                throw new ValidationException($"Máximo de {_settings.MaxProfileImageSizeInMb}MB");
        }
    }
}
