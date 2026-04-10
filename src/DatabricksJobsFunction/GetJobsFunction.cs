using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DatabricksJobsFunction;

public class GetJobsFunction
{
    private readonly DatabricksService _databricks;
    private readonly ILogger<GetJobsFunction> _logger;

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GetJobsFunction(DatabricksService databricks, ILogger<GetJobsFunction> logger)
    {
        _databricks = databricks;
        _logger = logger;
    }

    [Function("GetJobs")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "jobs")] HttpRequestData req)
    {
        _logger.LogInformation("GetJobs triggered");

        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        var search = query["search"];
        var limitStr = query["limit"];
        var limit = int.TryParse(limitStr, out var l) ? Math.Clamp(l, 1, 200) : 50;

        try
        {
            var jobs = await _databricks.GetJobPostingsAsync(limit, search);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(new
            {
                count = jobs.Count,
                jobs
            }, _json));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve jobs");
            var error = req.CreateResponse(HttpStatusCode.InternalServerError);
            await error.WriteStringAsync(JsonSerializer.Serialize(new { error = ex.Message }));
            return error;
        }
    }

    [Function("GetJobById")]
    public async Task<HttpResponseData> GetById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "jobs/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("GetJobById triggered for {Id}", id);

        try
        {
            var job = await _databricks.GetJobByIdAsync(id);

            if (job is null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync(JsonSerializer.Serialize(new { error = "Job not found" }));
                return notFound;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(job, _json));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve job {Id}", id);
            var error = req.CreateResponse(HttpStatusCode.InternalServerError);
            await error.WriteStringAsync(JsonSerializer.Serialize(new { error = ex.Message }));
            return error;
        }
    }
}
