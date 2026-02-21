namespace MyFinances.App.Abstractions
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(
            string fileName,
            Stream fileStream,
            string contentType);

        Task<Stream> DownloadAsync(string fileName);

        Task DeleteAsync(string fileName);
    }

}
