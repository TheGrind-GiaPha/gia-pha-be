using GiaPha.Services.Handlers;
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
        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAuditRestoreService, AuditRestoreService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPublicClanService, PublicClanService>();
        services.AddScoped<IClanRoleService, ClanRoleService>();

        // Handler thay đổi cho Luồng 1 (ChangeRequest.Apply).
        services.AddScoped<IChangeHandler, AddPersonHandler>();
        services.AddScoped<IChangeHandler, MoveSubtreeHandler>();
        services.AddScoped<IChangeHandler, DeletePersonHandler>();
        services.AddScoped<IChangeHandler, MergePersonsHandler>();

        return services;
    }
}
