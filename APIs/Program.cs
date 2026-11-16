using System.Text;
using GiaPha.APIs.Middleware;
using GiaPha.DataAccessObjects.Context;
using GiaPha.DataAccessObjects.Extensions;
using GiaPha.DataAccessObjects.Seed;
using GiaPha.Repositories.Extensions;
using GiaPha.Services.Extensions;
using GiaPha.Services.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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

    // Cho phép nhập JWT trong Swagger để test endpoint [Authorize].
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Dán JWT (không cần chữ 'Bearer ')."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ProblemDetails + global exception handler.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddAuthorization();

// JWT: cấu hình dùng chung cho AuthService (phát token) và middleware (kiểm token).
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Thiếu cấu hình 'Jwt' trong appsettings.json.");
builder.Services.AddSingleton(jwtSettings);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Trả ProblemDetails thay vì 401 trống để client hiển thị đúng lý do.
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(new
                {
                    title = "Chưa đăng nhập",
                    detail = "Phiên đăng nhập không hợp lệ hoặc đã hết hạn.",
                    status = StatusCodes.Status401Unauthorized
                });
            }
        };
    });

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
app.UseAuthentication();
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
