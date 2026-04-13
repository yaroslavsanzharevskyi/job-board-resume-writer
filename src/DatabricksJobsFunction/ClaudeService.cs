using Anthropic;
using Anthropic.Models.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DatabricksJobsFunction;

public class ClaudeService
{
    private readonly AnthropicClient _client;
    private readonly IConfiguration _config;
    private readonly ILogger<ClaudeService> _logger;

    public ClaudeService(IConfiguration config, ILogger<ClaudeService> logger)
    {
        _config = config;
        _logger = logger;

        _client = new AnthropicClient
        {
            ApiKey = config["ANTHROPIC_API_KEY"]
        };
    }

    public async Task<ClaudeOutput> GenerateResumeAsync(JobPosting job, string cvText)
    {
        _logger.LogInformation("Generating resume for job: {Title} at {Company}", job.Title, job.Company);

        var modelId = _config["CLAUDE_MODEL"] ?? "claude-sonnet-4-6";

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
            **Contract:** {job.ContractType ?? "Not specified"} {job.ContractTime ?? ""}
            **Salary:** {FormatSalary(job.SalaryMin, job.SalaryMax, job.SalaryMid)}
            **Category:** {job.Category ?? "Not specified"}
            **Seniority:** {job.Seniority ?? "Not specified"}
            **Work mode:** {job.WorkMode ?? "Not specified"}
            **Key skills required:** {(job.Skills.Length > 0 ? string.Join(", ", job.Skills) : "Not specified")}

            ### Job Description
            {job.Description}

            ---

            ## Candidate's Current CV

            {cvText}

            ---

            Please generate a tailored resume in markdown format that positions this candidate
            strongly for the above role. Substitute technology names in original resume to ones mentioned in the job description where applicable.
            Focus on relevance and impact.
            """;

        var parameters = new MessageCreateParams
        {
            Model = modelId,
            MaxTokens = 8000,
            Thinking = new ThinkingConfigAdaptive(),
            Messages =
            [
                new() { Role = Role.User, Content = userPrompt }
            ],
            System = systemPrompt,
        };

        // Stream to avoid HTTP timeout on long outputs
        var fullText = new System.Text.StringBuilder();
        int totalTokens = 0;

        await foreach (var streamEvent in _client.Messages.CreateStreaming(parameters))
        {
            if (streamEvent.TryPickContentBlockDelta(out var delta) &&
                delta.Delta.TryPickText(out var text))
            {
                fullText.Append(text.Text);
            }
            else if (streamEvent.TryPickDelta(out var msgDelta) &&
                     msgDelta.Usage is { } usage)
            {
                totalTokens = (int)usage.OutputTokens;
            }
        }

        var resumeMarkdown = fullText.ToString();

        _logger.LogInformation("Resume generated successfully, {Tokens} output tokens", totalTokens);

        return new ClaudeOutput(resumeMarkdown, totalTokens, DateTime.UtcNow);
    }

    private static string FormatSalary(string? min, string? max, string? mid) =>
        (min, max, mid) switch
        {
            (_, _, string m)   => $"~{m} (midpoint)",
            (string mn, string mx, _) => $"{mn} – {mx}",
            (string mn, null, _) => $"From {mn}",
            (null, string mx, _) => $"Up to {mx}",
            _ => "Not specified"
        };
}
