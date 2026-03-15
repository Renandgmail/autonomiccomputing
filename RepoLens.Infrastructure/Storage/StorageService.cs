using Amazon.S3;
using Amazon.S3.Model;
using RepoLens.Core.Exceptions;

namespace RepoLens.Infrastructure.Storage;

public class StorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public StorageService(IAmazonS3 s3Client, string bucketName)
    {
        _s3Client = s3Client;
        _bucketName = bucketName;
    }

    public async Task<string> UploadAsync(string key, byte[] content, CancellationToken ct = default)
    {
        try
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = new MemoryStream(content),
                ContentType = "application/octet-stream"
            };

            var response = await _s3Client.PutObjectAsync(putRequest, ct);
            
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new StorageException($"Failed to upload file {key} to storage");
            }

            return key;
        }
        catch (Exception ex)
        {
            throw new StorageException($"Failed to upload file {key} to storage", ex);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectMetadataAsync(request, ct);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            throw new StorageException($"Failed to check if file {key} exists in storage", ex);
        }
    }
}