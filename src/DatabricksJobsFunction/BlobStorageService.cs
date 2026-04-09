using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;

namespace DatabricksJobsFunction;

public class BlobStorageService
{
    private readonly BlobServiceClient _client;
    private readonly string _containerName;

    public BlobStorageService(IConfiguration config, TokenCredential credential)
    {
        // BLOB_STORAGE_URL = https://<account>.blob.core.windows.net (set by Terraform)
        // Falls back to Azurite for local development.
        var url = config["BLOB_STORAGE_URL"] ?? "http://127.0.0.1:10000/devstoreaccount1";
        _containerName = config["BLOB_CONTAINER_NAME"] ?? "resumes";
        _client = new BlobServiceClient(new Uri(url), credential);
    }

    public async Task<string> UploadResumeAsync(string blobName, byte[] content, string contentType)
    {
        var container = _client.GetBlobContainerClient(_containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None);

        var blob = container.GetBlobClient(blobName);
        using var stream = new MemoryStream(content);
        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType });

        // User-delegation SAS (managed identity — max 7 days)
        var now = DateTimeOffset.UtcNow;
        var delegationKey = await _client.GetUserDelegationKeyAsync(now, now.AddDays(7));

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = now.AddDays(7)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sas = sasBuilder.ToSasQueryParameters(delegationKey, _client.AccountName);
        return $"{blob.Uri}?{sas}";
    }

    public async Task DeleteBlobAsync(string blobName)
    {
        var container = _client.GetBlobContainerClient(_containerName);
        await container.GetBlobClient(blobName).DeleteIfExistsAsync();
    }
}
