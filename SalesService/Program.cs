using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SalesService.Data;
using SalesService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var configuredConnectionString =
    builder.Configuration.GetConnectionString("SalesDatabase")
    ?? throw new InvalidOperationException("Connection string 'SalesDatabase' is missing.");
var connectionStringBuilder = new SqliteConnectionStringBuilder(configuredConnectionString);
if (!Path.IsPathRooted(connectionStringBuilder.DataSource))
{
    connectionStringBuilder.DataSource =
        Path.Combine(builder.Environment.ContentRootPath, connectionStringBuilder.DataSource);
}

var databaseDirectory = Path.GetDirectoryName(connectionStringBuilder.DataSource);
if (!string.IsNullOrEmpty(databaseDirectory))
{
    Directory.CreateDirectory(databaseDirectory);
}

builder.Services.AddDbContext<SalesDbContext>(options =>
    options.UseSqlite(connectionStringBuilder.ConnectionString));

var repairServiceBaseUrl = builder.Configuration["RepairService:BaseUrl"]
    ?? throw new InvalidOperationException("Configuration 'RepairService:BaseUrl' is missing.");
if (!Uri.TryCreate(repairServiceBaseUrl, UriKind.Absolute, out var repairServiceUri)
    || repairServiceUri.Scheme is not "http" and not "https")
{
    throw new InvalidOperationException(
        "'RepairService:BaseUrl' must be an absolute HTTP or HTTPS URL.");
}

builder.Services.AddHttpClient<RepairClient>(client =>
{
    client.BaseAddress = repairServiceUri;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();
