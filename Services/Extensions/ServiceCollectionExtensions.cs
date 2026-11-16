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
        services.AddScoped<PrivacyFilterService>();
        services.AddScoped<PersonProfileService>();
        services.AddScoped<AnniversaryService>();
        services.AddSingleton<ILunarConverter, LunarConverter>();
        services.AddSingleton<IAnniversaryCalculator, AnniversaryCalculator>();

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
        services.AddScoped<IChangeRequestService, ChangeRequestService>();

        // Flow 4: Import & Export Services
        services.AddScoped<IExcelImportService, ExcelImportService>();
        services.AddScoped<IWordImportService, WordImportService>();
        services.AddHttpClient<ILlmExtractionService, GeminiExtractionService>();
        services.AddScoped<IDuplicateDetectionService, DuplicateDetectionService>();
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<IExportService, ExportService>();

        // Handler thay đổi cho ChangeRequest.Apply (Luồng 1 & Luồng 4).
        services.AddScoped<IChangeHandler, AddPersonHandler>();
        services.AddScoped<IChangeHandler, MoveSubtreeHandler>();
        services.AddScoped<IChangeHandler, DeletePersonHandler>();
        services.AddScoped<IChangeHandler, MergePersonsHandler>();
        services.AddScoped<IChangeHandler, AddUnionHandler>();
        services.AddScoped<IChangeHandler, ImportBatchChangeHandler>();

        return services;
    }
}
