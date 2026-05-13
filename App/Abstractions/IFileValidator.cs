namespace MyFinances.App.Abstractions
{
    public interface IFileValidator
    {
        void ValidateProfileImage(IFormFile file);
    }
}
