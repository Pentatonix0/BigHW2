using Microsoft.EntityFrameworkCore;
using FileAnalysisService.Services;
using FileAnalysisService.Clients;
using FileAnalysisService.Database;

var builder = WebApplication.CreateBuilder(args);

// Configure database connection string and DbContext with PostgreSQL provider
var connectionString = builder.Configuration.GetConnectionString("AppDatabase")
    ?? throw new InvalidOperationException("Connection string 'AppDatabase' not found.");

builder.Services.AddDbContext<FileAnalysisDBContext>(options =>
    options.UseNpgsql(connectionString));

// Configure HttpClient for FileStoringServiceClient with base URL and timeout
builder.Services.AddHttpClient<IFileStoringServiceClient, FileStoringServiceClient>(client =>
{
    var url = builder.Configuration["ServiceUrls:FileStoringService"]
        ?? throw new InvalidOperationException("FileStoringService URL is not configured.");
    client.BaseAddress = new Uri(url.EndsWith("/") ? url : url + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Configure HttpClient for WordCloudApiClient with base URL and timeout
builder.Services.AddHttpClient<IWordCloudApiClient, WordCloudApiClient>(client =>
{
    var url = builder.Configuration["WordCloudApi:BaseUrl"]
        ?? throw new InvalidOperationException("WordCloudApi URL is not configured.");
    client.BaseAddress = new Uri(url.EndsWith("/") ? url : url + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register application services for dependency injection
builder.Services.AddScoped<IFileAnalysisService, TextAnalysisService>();
builder.Services.AddScoped<IFilesPlagiarismService, FilePlagiarismService>();
builder.Services.AddScoped<IFileWordCloudService, FileWordCloudService>();
builder.Services.AddScoped<IFileAnalysisStorageService, FileAnalysisStorageService>();

// Enable CORS policy to allow any origin, method, and header (for development/testing)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add MVC controllers
builder.Services.AddControllers();

// Enable OpenAPI/Swagger generation and UI for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FileAnalysisService API",
        Version = "v1"
    });
});

var app = builder.Build();

// Enable Swagger UI and apply database migrations automatically in Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FileAnalysisService API V1"));

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<FileAnalysisDBContext>();
        try
        {
            dbContext.Database.Migrate(); // Apply pending EF Core migrations
        }
        catch (Exception)
        {
            throw new InvalidOperationException("Failed to apply database migrations.");
        }
    }
}

// Global exception handler middleware returns a generic 500 error response
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"message\":\"An unexpected error occurred on the server.\"}");
    });
});

// Enable CORS using the configured "AllowAll" policy
app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

// Run the application
app.Run();
