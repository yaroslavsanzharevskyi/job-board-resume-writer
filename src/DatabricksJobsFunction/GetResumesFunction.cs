using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DatabricksJobsFunction;

public class GetResumesFunction
{
    private readonly CosmosDbService _cosmos;
    private readonly ILogger<GetResumesFunction> _logger;

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GetResumesFunction(
        CosmosDbService cosmos,
        ILogger<GetResumesFunction> logger)
    {
        _cosmos = cosmos;
        _logger = logger;
    }

    [Function("GetResumes")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "resumes")] HttpRequestData req)
    {
        _logger.LogInformation("GetResumes triggered");

        try
        {
            var docs = await _cosmos.ListResumesAsync();

            var items = docs.Select(doc => new StoredResumeListItem(
                Id: doc.Id,
                JobId: doc.JobId,
                JobTitle: doc.JobTitle,
                Company: doc.Company,
                DownloadUrl: doc.BlobUrl,
                TokensUsed: doc.TokensUsed,
                GeneratedAt: doc.GeneratedAt
            )).ToArray();

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(new { resumes = items }, _json));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list resumes");
            var error = req.CreateResponse(HttpStatusCode.InternalServerError);
            await error.WriteStringAsync(JsonSerializer.Serialize(new { error = ex.Message }, _json));
            return error;
        }
    }
}
