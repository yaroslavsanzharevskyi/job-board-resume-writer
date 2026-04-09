using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;

namespace DatabricksJobsFunction;

public class BlobStorageService
{
    private readonly BlobServiceClient _client;
    private readonly string _containerName;

    public BlobStorageService(IConfiguration config)
    {
        var connectionString = config["BLOB_STORAGE_CONNECTION_STRING"] ?? "UseDevelopmentStorage=true";
        _containerName = config["BLOB_CONTAINER_NAME"] ?? "resumes";
        _client = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadResumeAsync(string blobName, byte[] content, string contentType)
    {
        var container = _client.GetBlobContainerClient(_containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None);

        var blob = container.GetBlobClient(blobName);
        using var stream = new MemoryStream(content);
        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });

        // Store a long-lived SAS URL (1 year) — avoids dependency on account-level public access
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddYears(1)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return blob.GenerateSasUri(sasBuilder).ToString();
    }

    public async Task DeleteBlobAsync(string blobName)
    {
        var container = _client.GetBlobContainerClient(_containerName);
        var blob = container.GetBlobClient(blobName);
        await blob.DeleteIfExistsAsync();
    }
}
