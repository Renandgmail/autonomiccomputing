using RepoLens.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace RepoLens.Infrastructure.Storage;

public class StorageService
{
    private readonly string _storageBasePath;
    private readonly ILogger<StorageService>? _logger;

    public StorageService(string storageBasePath, ILogger<StorageService>? logger = null)
    {
        _storageBasePath = storageBasePath;
        _logger = logger;
        
        // Ensure the storage directory exists
        if (!Directory.Exists(_storageBasePath))
        {
            Directory.CreateDirectory(_storageBasePath);
            _logger?.LogInformation("Created storage directory: {StorageBasePath}", _storageBasePath);
        }
    }

    public async Task<string> UploadAsync(string key, byte[] content, CancellationToken ct = default)
    {
        try
        {
            // Create content-addressable storage structure: /storage/objects/ab/abcd1234...
            var objectsPath = Path.Combine(_storageBasePath, "objects");
            var subDirectory = key.Length >= 2 ? key.Substring(0, 2) : "00";
            var directoryPath = Path.Combine(objectsPath, subDirectory);
            
            // Ensure the subdirectory exists
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = Path.Combine(directoryPath, key);
            
            // Only write if file doesn't already exist (content-addressable storage deduplication)
            if (!File.Exists(filePath))
            {
                await File.WriteAllBytesAsync(filePath, content, ct);
                _logger?.LogDebug("Stored file {Key} at {FilePath}", key, filePath);
            }
            else
            {
                _logger?.LogDebug("File {Key} already exists at {FilePath}, skipping write", key, filePath);
            }

            return key;
        }
        catch (Exception ex)
        {
            throw new StorageException($"Failed to upload file {key} to local storage", ex);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        try
        {
            var objectsPath = Path.Combine(_storageBasePath, "objects");
            var subDirectory = key.Length >= 2 ? key.Substring(0, 2) : "00";
            var directoryPath = Path.Combine(objectsPath, subDirectory);
            var filePath = Path.Combine(directoryPath, key);
            
            return File.Exists(filePath);
        }
        catch (Exception ex)
        {
            throw new StorageException($"Failed to check if file {key} exists in local storage", ex);
        }
    }

    public async Task<byte[]> DownloadAsync(string key, CancellationToken ct = default)
    {
        try
        {
            var objectsPath = Path.Combine(_storageBasePath, "objects");
            var subDirectory = key.Length >= 2 ? key.Substring(0, 2) : "00";
            var directoryPath = Path.Combine(objectsPath, subDirectory);
            var filePath = Path.Combine(directoryPath, key);
            
            if (!File.Exists(filePath))
            {
                throw new StorageException($"File {key} not found in local storage");
            }

            return await File.ReadAllBytesAsync(filePath, ct);
        }
        catch (Exception ex) when (ex is not StorageException)
        {
            throw new StorageException($"Failed to download file {key} from local storage", ex);
        }
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        try
        {
            var objectsPath = Path.Combine(_storageBasePath, "objects");
            var subDirectory = key.Length >= 2 ? key.Substring(0, 2) : "00";
            var directoryPath = Path.Combine(objectsPath, subDirectory);
            var filePath = Path.Combine(directoryPath, key);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger?.LogDebug("Deleted file {Key} from {FilePath}", key, filePath);
            }
        }
        catch (Exception ex)
        {
            throw new StorageException($"Failed to delete file {key} from local storage", ex);
        }
    }
}
