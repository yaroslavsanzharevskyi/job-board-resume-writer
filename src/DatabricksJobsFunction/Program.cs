using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DatabricksJobsFunction;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {

        services.AddHttpClient();
        services.AddScoped<DatabricksService>();
        services.AddScoped<ClaudeService>();
        services.AddSingleton<PdfService>();
        services.AddSingleton<BlobStorageService>();
        services.AddSingleton<CosmosDbService>();
    })
    .Build();

host.Run();
