using GiaPha.Services.Implementations;
using GiaPha.Services.Interfaces;
using GiaPha.Services.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace GiaPha.Services.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Singleton pattern: logger dùng chung toàn ứng dụng.
        services.AddSingleton<ILoggerManager, LoggerManager>();

        // Service nghiệp vụ là scoped vì phụ thuộc repository/DbContext scoped.
        services.AddScoped<IClanService, ClanService>();

        return services;
    }
}
