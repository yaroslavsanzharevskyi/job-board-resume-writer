using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DatabricksJobsFunction;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        // Single credential instance shared across all services; picks up
        // AZURE_CLIENT_ID automatically (set by Terraform via managed identity).
        services.AddSingleton<TokenCredential>(
            new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")
            }));

        services.AddHttpClient();
        services.AddScoped<DatabricksService>();
        services.AddScoped<ClaudeService>();
        services.AddSingleton<PdfService>();
        services.AddSingleton<BlobStorageService>();
        services.AddSingleton<CosmosDbService>();
    })
    .Build();

host.Run();
