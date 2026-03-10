namespace MyFinances.Infrastructure.Configuration
{
    public class SwaggerSettings
    {
        public string Title { get; set; } = "My Finances API";
        public string Version { get; set; } = "v1";
        public string Description { get; set; } = "API for managing personal finances";
        public string ContactName { get; set; } = "My Finances Team";
        public string ContactEmail { get; set; } = "contact@myfinances.com";
    }
}
