using GiaPha.APIs.Middleware;
using GiaPha.DataAccessObjects.Context;
using GiaPha.DataAccessObjects.Extensions;
using GiaPha.DataAccessObjects.Seed;
using GiaPha.Repositories.Extensions;
using GiaPha.Services.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "VGMS API",
        Version = "v1",
        Description = "API khung cho Hệ thống Quản lý Gia phả Dòng họ Việt Nam (VGMS)."
    });
});

// ProblemDetails + global exception handler.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

// Composition root: API chỉ lắp ghép các layer, không chứa logic DB.
builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddBusinessServices();

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<GiaPhaDbContext>("database");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseStaticFiles();
app.UseCors("Frontend");
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Seed dữ liệu mẫu (gia phả nhà Nguyễn) khi database còn trống.
using (var scope = app.Services.CreateScope())
{
    try
    {
        var seeder = scope.ServiceProvider.GetRequiredService<GenealogySeeder>();
        var seeded = await seeder.SeedAsync();
        if (seeded > 0)
        {
            app.Logger.LogInformation("Đã seed {Count} người cho gia phả nhà Nguyễn.", seeded);
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Không thể seed dữ liệu mẫu. Kiểm tra kết nối database.");
    }
}

app.Run();

// Cần cho WebApplicationFactory trong integration test sau này.
public partial class Program;
