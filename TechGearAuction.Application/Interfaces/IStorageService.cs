namespace TechGearAuction.Application.Interfaces;

public interface IStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string bucketName);
    Task InitializeBucketsAsync();
}

