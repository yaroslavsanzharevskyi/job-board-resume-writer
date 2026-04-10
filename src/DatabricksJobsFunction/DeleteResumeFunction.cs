using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DatabricksJobsFunction;

public class DeleteResumeFunction
{
    private readonly CosmosDbService _cosmos;
    private readonly BlobStorageService _blob;
    private readonly ILogger<DeleteResumeFunction> _logger;

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public DeleteResumeFunction(
        CosmosDbService cosmos,
        BlobStorageService blob,
        ILogger<DeleteResumeFunction> logger)
    {
        _cosmos = cosmos;
        _blob = blob;
        _logger = logger;
    }

    [Function("DeleteResume")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "resumes/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("DeleteResume triggered for {ResumeId}", id);

        try
        {
            var doc = await _cosmos.GetResumeAsync(id);
            if (doc is null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync(JsonSerializer.Serialize(new { error = $"Resume {id} not found" }, _json));
                return notFound;
            }

            var blobName = $"resumes/{id}.pdf";
            await _blob.DeleteBlobAsync(blobName);
            await _cosmos.DeleteResumeAsync(id);

            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete resume {ResumeId}", id);
            var error = req.CreateResponse(HttpStatusCode.InternalServerError);
            await error.WriteStringAsync(JsonSerializer.Serialize(new { error = ex.Message }, _json));
            return error;
        }
    }
}
