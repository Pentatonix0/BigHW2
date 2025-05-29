using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure reverse proxy (YARP) from appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// HTTP client for downloading Swagger documents from services
builder.Services.AddHttpClient("swagger_downloader")
    .AddStandardResilienceHandler(); // Enables retry and timeout handling

builder.Services.AddEndpointsApiExplorer();

// Setup Swagger generator for the aggregated API only
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("aggregated-v1", new OpenApiInfo
    {
        Title = "API Gateway (Aggregated Services)",
        Version = "v1",
        Description = "Aggregated API from FileStoringService and FileAnalysisService"
    });

    // Prevent the gateway's own endpoints from appearing in Swagger
    options.DocInclusionPredicate((_, _) => false);
});

var app = builder.Build();

// Enable reverse proxy routing
app.MapReverseProxy();

// Setup Swagger UI and aggregation logic for development environment only
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/custom/aggregated-v1/swagger.json", "Aggregated API V1");
        options.RoutePrefix = "swagger";
    });

    // Aggregated Swagger endpoint that merges documents from other services
    app.MapGet("/swagger/custom/aggregated-v1/swagger.json", async (
        IConfiguration config,
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory) =>
    {
        var logger = loggerFactory.CreateLogger("SwaggerAggregator");

        // Initialize the aggregated Swagger document
        var combinedDoc = new OpenApiDocument
        {
            Info = new OpenApiInfo { Title = "Aggregated API", Version = "v1" },
            Paths = new OpenApiPaths(),
            Components = new OpenApiComponents { Schemas = new Dictionary<string, OpenApiSchema>() },
            Servers = new List<OpenApiServer> { new OpenApiServer { Url = "/" } }
        };

        var endpoints = config.GetSection("SwaggerEndpoints").Get<List<SwaggerEndpointConfig>>();
        var httpClient = httpClientFactory.CreateClient("swagger_downloader");

        // Loop through each service Swagger endpoint and merge its document
        foreach (var endpoint in endpoints ?? Enumerable.Empty<SwaggerEndpointConfig>())
        {
            try
            {
                var response = await httpClient.GetAsync(endpoint.Url);
                if (!response.IsSuccessStatusCode) continue;

                using var stream = await response.Content.ReadAsStreamAsync();
                var reader = new OpenApiStreamReader();
                var doc = reader.Read(stream, out _);

                var schemaMapping = new Dictionary<string, string>();

                // Add and rename schemas to avoid name conflicts
                foreach (var schema in doc.Components?.Schemas ?? new Dictionary<string, OpenApiSchema>())
                {
                    var newName = $"{endpoint.Key}_{schema.Key}";
                    combinedDoc.Components.Schemas[newName] = schema.Value;
                    schemaMapping[schema.Key] = newName;
                }

                // Rewrite paths and apply schema reference mappings
                foreach (var path in doc.Paths)
                {
                    var newPath = path.Key.StartsWith(endpoint.ServicePathPrefixToReplace)
                        ? endpoint.GatewayPathPrefix.TrimEnd('/') + path.Key[endpoint.ServicePathPrefixToReplace.Length..]
                        : endpoint.GatewayPathPrefix.TrimEnd('/') + path.Key;

                    foreach (var op in path.Value.Operations.Values)
                    {
                        foreach (var p in op.Parameters ?? []) UpdateRef(p.Schema?.Reference, schemaMapping);
                        foreach (var r in op.Responses.Values)
                            foreach (var c in r.Content.Values)
                                UpdateRef(c.Schema?.Reference, schemaMapping);
                        foreach (var c in op.RequestBody?.Content.Values ?? [])
                            UpdateRef(c.Schema?.Reference, schemaMapping);
                    }

                    if (!combinedDoc.Paths.ContainsKey(newPath))
                        combinedDoc.Paths[newPath] = path.Value;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error aggregating Swagger from {Service}", endpoint.Key);
            }
        }

        // Helper method to update $ref schema identifiers after renaming
        static void UpdateRef(OpenApiReference? reference, Dictionary<string, string> mapping)
        {
            if (reference?.Id != null && mapping.TryGetValue(reference.Id, out var newId))
            {
                reference.Id = newId;
            }
        }

        // Serialize the aggregated document and return as JSON
        var streamOut = new MemoryStream();
        var writer = new StreamWriter(streamOut, new UTF8Encoding(false));
        var jsonWriter = new Microsoft.OpenApi.Writers.OpenApiJsonWriter(writer);
        combinedDoc.SerializeAsV3(jsonWriter);
        writer.Flush();
        streamOut.Position = 0;

        return Results.Stream(streamOut, "application/json");
    }).WithName("GetAggregatedSwaggerJson");
}

app.Run();

// Configuration model for describing individual Swagger service endpoints
public class SwaggerEndpointConfig
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string GatewayPathPrefix { get; set; } = string.Empty;
    public string ServicePathPrefixToReplace { get; set; } = string.Empty;
}
