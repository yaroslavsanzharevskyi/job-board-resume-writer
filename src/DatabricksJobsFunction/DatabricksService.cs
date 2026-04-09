using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DatabricksJobsFunction;

public class DatabricksService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<DatabricksService> _logger;

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public DatabricksService(IHttpClientFactory factory, IConfiguration config, ILogger<DatabricksService> logger)
    {
        _http = factory.CreateClient();
        _config = config;
        _logger = logger;

        var host = config["DATABRICKS_HOST"]!.TrimEnd('/');
        var token = config["DATABRICKS_TOKEN"]!;

        _http.BaseAddress = new Uri(host);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private const string SelectColumns = """
        id,
        title,
        company_display_name  AS company,
        location_display_name AS location,
        description,
        CAST(salary_min AS STRING) AS salary_min,
        CAST(salary_max AS STRING) AS salary_max,
        CAST(salary_mid AS STRING) AS salary_mid,
        contract_type,
        contract_time,
        category_label         AS category,
        category_tag,
        CAST(created_at AS STRING) AS created_at,
        TO_JSON(skills)        AS skills_json,
        seniority,
        work_mode,
        redirect_url,
        source_country
        """;

    public async Task<List<JobPosting>> GetJobPostingsAsync(int limit = 50, string? searchTerm = null)
    {
        var (warehouseId, catalog, schema, table) = GetConfig();

        var whereClause = string.IsNullOrEmpty(searchTerm)
            ? ""
            : $"WHERE lower(title) LIKE lower('%{searchTerm}%') OR lower(description) LIKE lower('%{searchTerm}%')";

        var sql = $"""
            SELECT {SelectColumns}
            FROM {catalog}.{schema}.{table}
            {whereClause}
            ORDER BY created_at DESC
            LIMIT {limit}
            """;

        var rows = await ExecuteQueryAsync(sql, warehouseId, catalog, schema);
        return rows.Select(MapRow).ToList();
    }

    public async Task<JobPosting?> GetJobByIdAsync(string id)
    {
        var (warehouseId, catalog, schema, table) = GetConfig();

        // Use a parameterized query to avoid SQL injection
        var sql = $"""
            SELECT {SelectColumns}
            FROM {catalog}.{schema}.{table}
            WHERE id = :job_id
            LIMIT 1
            """;

        var rows = await ExecuteQueryAsync(sql, warehouseId, catalog, schema,
            parameters: [new { name = "job_id", value = id, type = "STRING" }]);

        return rows.Select(MapRow).FirstOrDefault();
    }

    private (string warehouseId, string catalog, string schema, string table) GetConfig() => (
        _config["DATABRICKS_HTTP_PATH"]!.Split('/').Last(),
        _config["DATABRICKS_CATALOG"]!,
        _config["DATABRICKS_SCHEMA"]!,
        _config["DATABRICKS_TABLE"]!
    );

    private async Task<string[][]> ExecuteQueryAsync(
        string sql,
        string warehouseId,
        string catalog,
        string schema,
        object[]? parameters = null)
    {
        var payload = new Dictionary<string, object>
        {
            ["statement"]      = sql,
            ["warehouse_id"]   = warehouseId,
            ["catalog"]        = catalog,
            ["schema"]         = schema,
            ["wait_timeout"]   = "30s",
            ["on_wait_timeout"] = "CANCEL"
        };
        if (parameters is { Length: > 0 })
            payload["parameters"] = parameters;

        var body = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("/api/2.0/sql/statements", body);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            _logger.LogError("Databricks error {StatusCode}: {Body}", response.StatusCode, err);
            throw new Exception($"Databricks query failed: {response.StatusCode}");
        }

        var result = JsonSerializer.Deserialize<DatabricksQueryResponse>(
            await response.Content.ReadAsStringAsync(), _json);

        if (result?.status.state != "SUCCEEDED")
        {
            var msg = result?.status.error?.message ?? "Unknown error";
            throw new Exception($"Query did not succeed: {msg}");
        }

        return result.result?.data_array ?? [];
    }

    private static JobPosting MapRow(string[] row) => new(
        Id:            row.ElementAtOrDefault(0) ?? "",
        Title:         row.ElementAtOrDefault(1) ?? "",
        Company:       row.ElementAtOrDefault(2) ?? "",
        Location:      row.ElementAtOrDefault(3) ?? "",
        Description:   row.ElementAtOrDefault(4) ?? "",
        SalaryMin:     row.ElementAtOrDefault(5),
        SalaryMax:     row.ElementAtOrDefault(6),
        SalaryMid:     row.ElementAtOrDefault(7),
        ContractType:  row.ElementAtOrDefault(8),
        ContractTime:  row.ElementAtOrDefault(9),
        Category:      row.ElementAtOrDefault(10),
        CategoryTag:   row.ElementAtOrDefault(11),
        CreatedAt:     row.ElementAtOrDefault(12) ?? "",
        Skills:        ParseSkills(row.ElementAtOrDefault(13)),
        Seniority:     row.ElementAtOrDefault(14),
        WorkMode:      row.ElementAtOrDefault(15),
        RedirectUrl:   row.ElementAtOrDefault(16),
        SourceCountry: row.ElementAtOrDefault(17)
    );

    private static string[] ParseSkills(string? json)
    {
        if (string.IsNullOrEmpty(json)) return [];
        try { return JsonSerializer.Deserialize<string[]>(json) ?? []; }
        catch { return []; }
    }
}

// Register as scoped service in Program.cs
