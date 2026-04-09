using System.Text.Json.Serialization;

namespace DatabricksJobsFunction;

public record JobPosting(
    string Id,
    string Title,
    string Company,
    string Location,
    string Description,
    string? SalaryMin,
    string? SalaryMax,
    string? SalaryMid,
    string? ContractType,
    string? ContractTime,
    string? Category,
    string? CategoryTag,
    string CreatedAt,
    string[] Skills,
    string? Seniority,
    string? WorkMode,
    string? RedirectUrl,
    string? SourceCountry
);

public record ResumeResult(
    string ResumeId,
    string JobId,
    string JobTitle,
    string Company,
    string ResumeMarkdown,
    string BlobUrl,
    int TokensUsed,
    DateTime GeneratedAt
);

// CosmosDB document — stored in the Resumes container with partition key "pk"
public class StoredResumeDocument
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("pk")]
    public string Pk { get; set; } = "resume";

    [JsonPropertyName("jobId")]
    public string JobId { get; set; } = default!;

    [JsonPropertyName("jobTitle")]
    public string JobTitle { get; set; } = default!;

    [JsonPropertyName("company")]
    public string Company { get; set; } = default!;

    [JsonPropertyName("blobUrl")]
    public string BlobUrl { get; set; } = default!;

    [JsonPropertyName("tokensUsed")]
    public int TokensUsed { get; set; }

    [JsonPropertyName("generatedAt")]
    public DateTime GeneratedAt { get; set; }
}

public record StoredResumeListItem(
    string Id,
    string JobId,
    string JobTitle,
    string Company,
    string DownloadUrl,
    int TokensUsed,
    DateTime GeneratedAt
);

public record GenerateResumeRequest(
    string JobId,
    string CvText
);

public record GenerateCustomResumeRequest(
    string JobTitle,
    string Company,
    string JobDescription,
    string CvText
);

// Intermediate output from ClaudeService — enriched by GenerateResumeFunction before returning to client
public record ClaudeOutput(string ResumeMarkdown, int TokensUsed, DateTime GeneratedAt);

// Databricks API shapes
public record DatabricksQueryResponse(
    string statement_id,
    DatabricksStatus status,
    DatabricksManifest? manifest,
    DatabricksResult? result
);

public record DatabricksStatus(string state, DatabricksError? error);
public record DatabricksError(string error_code, string message);
public record DatabricksManifest(DatabricksSchema schema);
public record DatabricksSchema(int column_count, DatabricksColumn[] columns);
public record DatabricksColumn(string name, string type_name);
public record DatabricksResult(string[][]? data_array);
