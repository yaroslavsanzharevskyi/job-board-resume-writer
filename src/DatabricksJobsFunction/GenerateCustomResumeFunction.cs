using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace DatabricksJobsFunction;

public class GenerateCustomResumeFunction
{
    private readonly ClaudeService _claude;
    private readonly PdfService _pdf;
    private readonly BlobStorageService _blob;
    private readonly CosmosDbService _cosmos;
    private readonly ILogger<GenerateCustomResumeFunction> _logger;

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GenerateCustomResumeFunction(
        ClaudeService claude,
        PdfService pdf,
        BlobStorageService blob,
        CosmosDbService cosmos,
        ILogger<GenerateCustomResumeFunction> logger)
    {
        _claude = claude;
        _pdf = pdf;
        _blob = blob;
        _cosmos = cosmos;
        _logger = logger;
    }

    [Function("GenerateCustomResume")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "resume/generate-custom")] HttpRequestData req)
    {
        _logger.LogInformation("GenerateCustomResume triggered");

        GenerateCustomResumeRequest? request;
        try
        {
            request = await JsonSerializer.DeserializeAsync<GenerateCustomResumeRequest>(req.Body, _json);

            if (request is null
                || string.IsNullOrWhiteSpace(request.JobTitle)
                || string.IsNullOrWhiteSpace(request.JobDescription)
                || string.IsNullOrWhiteSpace(request.CvText))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync(JsonSerializer.Serialize(new
                {
                    error = "Request body must include jobTitle, jobDescription and cvText"
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
            // Build a synthetic JobPosting so ClaudeService can be reused without changes
            var syntheticJob = new JobPosting(
                Id: Guid.NewGuid().ToString(),
                Title: request.JobTitle,
                Company: request.Company ?? "Unknown",
                Location: string.Empty,
                Description: request.JobDescription,
                SalaryMin: null,
                SalaryMax: null,
                SalaryMid: null,
                ContractType: null,
                ContractTime: null,
                Category: null,
                CategoryTag: null,
                CreatedAt: DateTime.UtcNow.ToString("o"),
                Skills: [],
                Seniority: null,
                WorkMode: null,
                RedirectUrl: null,
                SourceCountry: null
            );

            var claude = await _claude.GenerateResumeAsync(syntheticJob, request.CvText);

            var resumeId = Guid.NewGuid().ToString();
            var pdfBytes = _pdf.ConvertMarkdownToPdf(claude.ResumeMarkdown);
            var blobName = $"resumes/{resumeId}.pdf";
            var blobUrl = await _blob.UploadResumeAsync(blobName, pdfBytes, "application/pdf");

            var doc = new StoredResumeDocument
            {
                Id = resumeId,
                JobId = syntheticJob.Id,
                JobTitle = syntheticJob.Title,
                Company = syntheticJob.Company,
                BlobUrl = blobUrl,
                TokensUsed = claude.TokensUsed,
                GeneratedAt = claude.GeneratedAt
            };
            await _cosmos.SaveResumeAsync(doc);

            var result = new ResumeResult(
                ResumeId: resumeId,
                JobId: syntheticJob.Id,
                JobTitle: syntheticJob.Title,
                Company: syntheticJob.Company,
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
            _logger.LogError(ex, "Failed to generate custom resume for job title {JobTitle}", request.JobTitle);
            var error = req.CreateResponse(HttpStatusCode.InternalServerError);
            await error.WriteStringAsync(JsonSerializer.Serialize(new { error = ex.Message }, _json));
            return error;
        }
    }
}
