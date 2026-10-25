using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RepairService.Data;
using RepairService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var configuredConnectionString =
    builder.Configuration.GetConnectionString("RepairDatabase")
    ?? throw new InvalidOperationException("Connection string 'RepairDatabase' is missing.");
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

builder.Services.AddDbContext<RepairDbContext>(options =>
    options.UseSqlite(connectionStringBuilder.ConnectionString));

var catalogBaseUrl = builder.Configuration["CatalogService:BaseUrl"]
    ?? throw new InvalidOperationException("Configuration 'CatalogService:BaseUrl' is missing.");
if (!Uri.TryCreate(catalogBaseUrl, UriKind.Absolute, out var catalogUri))
{
    throw new InvalidOperationException("'CatalogService:BaseUrl' must be an absolute URL.");
}
if (catalogUri.Scheme is not "http" and not "https")
{
    throw new InvalidOperationException("'CatalogService:BaseUrl' must use HTTP or HTTPS.");
}

builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = catalogUri;
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
    var dbContext = scope.ServiceProvider.GetRequiredService<RepairDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();
