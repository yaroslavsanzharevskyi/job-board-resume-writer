using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace DatabricksJobsFunction;

public class CosmosDbService
{
    private readonly CosmosClient _client;
    private readonly string _databaseName;
    private readonly string _containerName;

    public CosmosDbService(IConfiguration config)
    {
        var connectionString = config["COSMOS_CONNECTION_STRING"]
            ?? "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b4RFZHkU9y7yABaQEZHBBrBPk3gzA==";
        _databaseName = config["COSMOS_DATABASE_NAME"] ?? "ResumeDb";
        _containerName = config["COSMOS_CONTAINER_NAME"] ?? "Resumes";

        _client = new CosmosClient(connectionString, new CosmosClientOptions
        {
            Serializer = new SystemTextJsonCosmosSerializer(
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
        });
    }

    public async Task EnsureContainerExistsAsync()
    {
        var dbResponse = await _client.CreateDatabaseIfNotExistsAsync(_databaseName);
        await dbResponse.Database.CreateContainerIfNotExistsAsync(_containerName, "/pk");
    }

    public async Task SaveResumeAsync(StoredResumeDocument doc)
    {
        await EnsureContainerExistsAsync();
        var container = _client.GetContainer(_databaseName, _containerName);
        await container.CreateItemAsync(doc, new PartitionKey(doc.Pk));
    }

    public async Task<StoredResumeDocument?> GetResumeAsync(string id)
    {
        await EnsureContainerExistsAsync();
        var container = _client.GetContainer(_databaseName, _containerName);
        try
        {
            var response = await container.ReadItemAsync<StoredResumeDocument>(id, new PartitionKey("resume"));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task DeleteResumeAsync(string id)
    {
        await EnsureContainerExistsAsync();
        var container = _client.GetContainer(_databaseName, _containerName);
        await container.DeleteItemAsync<StoredResumeDocument>(id, new PartitionKey("resume"));
    }

    public async Task<List<StoredResumeDocument>> ListResumesAsync()
    {
        await EnsureContainerExistsAsync();
        var container = _client.GetContainer(_databaseName, _containerName);
        var query = new QueryDefinition(
            "SELECT * FROM c ORDER BY c.generatedAt DESC OFFSET 0 LIMIT 50");

        var results = new List<StoredResumeDocument>();
        using var iterator = container.GetItemQueryIterator<StoredResumeDocument>(query);
        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync();
            results.AddRange(page);
        }
        return results;
    }
}
