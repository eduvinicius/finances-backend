using System.Net.Sockets;
using MyFinances.App.Abstractions;

namespace MyFinances.Infrasctructure.Storage
{
    public class SupabaseStorageService(HttpClient httpClient) : IFileStorageService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly string _bucketName = "ProfileImage";

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

            return $"https://mzzvhtvojiqbvhitkcbw.supabase.co/storage/v1/object/public/{_bucketName}/{fileName}";
        }
    }
}
