using Microsoft.EntityFrameworkCore;
using FileStoringService.Database;
using FileStoringService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure FileStorageSettings
builder.Services.Configure<FileStorageSettings>(
    builder.Configuration.GetSection("FileStorage")
);

// Configure PostgreSQL database
var connectionString = builder.Configuration.GetConnectionString("AppDatabase");
builder.Services.AddDbContext<FileDBContext>(options =>
    options.UseNpgsql(connectionString)
);

// Register FileStorageService
builder.Services.AddScoped<IFileStoringService, FileStorageService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FileStoringService API"));

    // Apply database migrations
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<FileDBContext>();
        try
        {
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database.");
        }
    }
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public class FileStorageSettings
{
    public string BasePath { get; set; } = string.Empty;
}
