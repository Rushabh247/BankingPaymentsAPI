using System.IO;

namespace BankingPaymentsAPI.Services.Storage
{
    public interface IFileStorageService
    {
        Task<(string Url, string PublicId)> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder);
        Task DeleteFileAsync(string publicId);
    }
}
