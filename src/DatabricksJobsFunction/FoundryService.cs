using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DatabricksJobsFunction;

public class FoundryService
{
    private readonly ChatCompletionsClient _client;
    private readonly IConfiguration _config;
    private readonly ILogger<FoundryService> _logger;

    public FoundryService(IConfiguration config, ILogger<FoundryService> logger)
    {
        _config = config;
        _logger = logger;

        var endpoint = new Uri(config["FOUNDRY_ENDPOINT"]!);
        var credential = new AzureKeyCredential(config["FOUNDRY_API_KEY"]!);

        _client = new ChatCompletionsClient(endpoint, credential);
    }

    public async Task<ResumeResult> GenerateResumeAsync(JobPosting job, string cvText)
    {
        _logger.LogInformation("Generating resume for job: {Title} at {Company}", job.Title, job.Company);

        var modelName = _config["FOUNDRY_DEPLOYMENT_NAME"] ?? "gpt-4o-resume";

        var systemPrompt = """
            You are an expert resume writer with 15 years of experience tailoring resumes 
            for specific job postings. Your goal is to rewrite and restructure the candidate's 
            existing CV to best match the job requirements — highlighting relevant experience, 
            reordering sections, and using keywords from the job description naturally.

            Rules:
            - Never invent experience or skills the candidate does not have
            - Mirror the language and keywords from the job description where truthful
            - Output a clean, ATS-friendly resume in markdown format
            - Sections: Summary, Experience, Skills, Education
            - Keep the summary to 3-4 sentences tailored to this specific role
            """;

        var userPrompt = $"""
            ## Job Posting

            **Title:** {job.Title}
            **Company:** {job.Company}
            **Location:** {job.Location}
            **Contract:** {job.ContractType ?? "Not specified"}
            **Salary:** {FormatSalary(job.SalaryMin, job.SalaryMax)}
            **Category:** {job.Category ?? "Not specified"}

            ### Job Description
            {job.Description}

            ---

            ## Candidate's Current CV

            {cvText}

            ---

            Please generate a tailored resume in markdown format that positions this candidate 
            strongly for the above role.
            """;

        var options = new ChatCompletionsOptions
        {
            Model = modelName,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            },
            MaxTokens = 2000,
            Temperature = 0.3f
        };

        var response = await _client.CompleteAsync(options);
        var resumeMarkdown = response.Value.Content ?? string.Empty;

        _logger.LogInformation("Resume generated successfully, {Tokens} tokens used",
            response.Value.Usage?.TotalTokens);

        return new ResumeResult(
            ResumeId: Guid.NewGuid().ToString(),
            BlobUrl: string.Empty, // This will be filled in later after uploading to blob storage
            JobId: job.Id,
            JobTitle: job.Title,
            Company: job.Company,
            ResumeMarkdown: resumeMarkdown,
            TokensUsed: response.Value.Usage?.TotalTokens ?? 0,
            GeneratedAt: DateTime.UtcNow
        );
    }

    private static string FormatSalary(string? min, string? max) =>
        (min, max) switch
        {
            (null, null) => "Not specified",
            (var m, null) => $"From {m}",
            (null, var m) => $"Up to {m}",
            (var mn, var mx) => $"{mn} - {mx}"
        };
}
