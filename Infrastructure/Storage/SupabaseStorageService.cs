using MyFinances.App.Abstractions;

namespace MyFinances.Infrastructure.Storage
{
    public class SupabaseStorageService(HttpClient httpClient, IConfiguration configuration) : IFileStorageService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly string _bucketName = configuration["Supabase:Bucket"]
            ?? throw new InvalidOperationException("Supabase:Bucket is not configured.");
        private readonly string _supabaseUrl = (configuration["Supabase:Url"]
            ?? throw new InvalidOperationException("Supabase:Url is not configured.")).TrimEnd('/');

        public async Task<string> UploadAsync(
            string fileName,
            Stream fileStream,
            string contentType)
        {
            var content = new StreamContent(fileStream);
            content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            var response = await _httpClient.PostAsync(
                $"storage/v1/object/{_bucketName}/{fileName}",
                content);

            response.EnsureSuccessStatusCode();

            return $"{_supabaseUrl}/storage/v1/object/public/{_bucketName}/{fileName}";
        }

        public async Task<Stream> DownloadAsync(string fileName)
        {
            var response = await _httpClient.GetAsync(
                $"storage/v1/object/public/{_bucketName}/{fileName}");

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task DeleteAsync(string fileName)
        {
            var response = await _httpClient.DeleteAsync(
                $"storage/v1/object/{_bucketName}/{fileName}");

            response.EnsureSuccessStatusCode();
        }
    }
}


