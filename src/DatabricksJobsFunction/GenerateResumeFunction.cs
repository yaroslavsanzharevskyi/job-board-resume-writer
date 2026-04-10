using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DatabricksJobsFunction;

public class GenerateResumeFunction
{
    private readonly DatabricksService _databricks;
    private readonly ClaudeService _claude;
    private readonly PdfService _pdf;
    private readonly BlobStorageService _blob;
    private readonly CosmosDbService _cosmos;
    private readonly ILogger<GenerateResumeFunction> _logger;

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GenerateResumeFunction(
        DatabricksService databricks,
        ClaudeService claude,
        PdfService pdf,
        BlobStorageService blob,
        CosmosDbService cosmos,
        ILogger<GenerateResumeFunction> logger)
    {
        _databricks = databricks;
        _claude = claude;
        _pdf = pdf;
        _blob = blob;
        _cosmos = cosmos;
        _logger = logger;
    }

    [Function("GenerateResume")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "resume/generate")] HttpRequestData req)
    {
        _logger.LogInformation("GenerateResume triggered");

        GenerateResumeRequest? request;
        try
        {
            request = await JsonSerializer.DeserializeAsync<GenerateResumeRequest>(req.Body, _json);

            if (request is null || string.IsNullOrEmpty(request.JobId) || string.IsNullOrEmpty(request.CvText))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync(JsonSerializer.Serialize(new
                {
                    error = "Request body must include jobId and cvText"
                }, _json));
                return bad;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid request body");
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync(JsonSerializer.Serialize(new { error = "Invalid JSON body" }, _json));
            return bad;
        }

        try
        {
            var job = await _databricks.GetJobByIdAsync(request.JobId);

            if (job is null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync(JsonSerializer.Serialize(new
                {
                    error = $"Job {request.JobId} not found"
                }, _json));
                return notFound;
            }

            var claude = await _claude.GenerateResumeAsync(job, request.CvText);

            var resumeId = Guid.NewGuid().ToString();
            var pdfBytes = _pdf.ConvertMarkdownToPdf(claude.ResumeMarkdown);
            var blobName = $"resumes/{resumeId}.pdf";
            var blobUrl = await _blob.UploadResumeAsync(blobName, pdfBytes, "application/pdf");

            var doc = new StoredResumeDocument
            {
                Id = resumeId,
                JobId = job.Id,
                JobTitle = job.Title,
                Company = job.Company,
                BlobUrl = blobUrl,
                TokensUsed = claude.TokensUsed,
                GeneratedAt = claude.GeneratedAt
            };
            await _cosmos.SaveResumeAsync(doc);

            var result = new ResumeResult(
                ResumeId: resumeId,
                JobId: job.Id,
                JobTitle: job.Title,
                Company: job.Company,
                ResumeMarkdown: claude.ResumeMarkdown,
                BlobUrl: blobUrl,
                TokensUsed: claude.TokensUsed,
                GeneratedAt: claude.GeneratedAt
            );

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(result, _json));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate resume for job {JobId}", request.JobId);
            var error = req.CreateResponse(HttpStatusCode.InternalServerError);
            await error.WriteStringAsync(JsonSerializer.Serialize(new { error = ex.Message }, _json));
            return error;
        }
    }
}
