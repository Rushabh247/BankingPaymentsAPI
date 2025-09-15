using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace BankingPaymentsAPI.Services.Storage
{
    public class CloudinaryFileStorageService : IFileStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryFileStorageService(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<(string Url, string PublicId)> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder)
        {
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = folder,
                PublicId = Guid.NewGuid().ToString() // unique id
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return (uploadResult.SecureUrl.ToString(), uploadResult.PublicId);
            }

            throw new Exception("File upload to Cloudinary failed");
        }

        public async Task DeleteFileAsync(string publicId)
        {
            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);

            if (result.Result != "ok")
                throw new Exception("File deletion from Cloudinary failed");
        }
    }
}
