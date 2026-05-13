namespace MyFinances.App.Configuration
{
    public class ApiSettings
    {
        public int MaxProfileImageSizeInMb { get; set; } = 5;
        public string[] AllowedImageContentTypes { get; set; } = ["image/jpeg", "image/png", "image/gif", "image/webp"];
    }
}
