using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using TechGearAuction.Application.Common.Models;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Infrastructure.Services;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly MinioSettings _settings;

    public MinioStorageService(IOptions<MinioSettings> settings)
    {
        _settings = settings.Value;

        var client = new MinioClient()
            .WithEndpoint(_settings.Endpoint)
            .WithCredentials(_settings.AccessKey, _settings.SecretKey);

        if (_settings.UseSSL)
        {
            client = client.WithSSL();
        }

        _minioClient = client.Build();
    }

    public async Task InitializeBucketsAsync()
    {
        var bucketsToCreate = new Dictionary<string, bool>
        {
            { _settings.Buckets.Avatars, true }, // true = Public Read
            { _settings.Buckets.Auctions, true },
            { _settings.Buckets.Evidences, false } // private
        };

        foreach (var bucket in bucketsToCreate)
        {
            var bucketName = bucket.Key;
            var isPublic = bucket.Value;

            var beArgs = new BucketExistsArgs().WithBucket(bucketName);
            bool found = await _minioClient.BucketExistsAsync(beArgs);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs().WithBucket(bucketName);
                await _minioClient.MakeBucketAsync(mbArgs);

                if (isPublic)
                {
                    // Set public read policy
                    string policy = $@"{{
                        ""Version"": ""2012-10-17"",
                        ""Statement"": [
                            {{
                                ""Effect"": ""Allow"",
                                ""Principal"": {{""AWS"": [""*""]}},
                                ""Action"": [""s3:GetObject""],
                                ""Resource"": [""arn:aws:s3:::{bucketName}/*""]
                            }}
                        ]
                    }}";
                    
                    var spArgs = new SetPolicyArgs()
                        .WithBucket(bucketName)
                        .WithPolicy(policy);
                    
                    await _minioClient.SetPolicyAsync(spArgs);
                }
            }
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string bucketName)
    {
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(fileName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putObjectArgs);

        var protocol = _settings.UseSSL ? "https" : "http";
        return $"{protocol}://{_settings.Endpoint}/{bucketName}/{fileName}";
    }
}

