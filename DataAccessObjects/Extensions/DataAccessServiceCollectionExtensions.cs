using GiaPha.BusinessObjects.Auditing;
using GiaPha.DataAccessObjects.Auditing;
using GiaPha.DataAccessObjects.Context;
using GiaPha.DataAccessObjects.DAOs;
using GiaPha.DataAccessObjects.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GiaPha.DataAccessObjects.Extensions;

public static class DataAccessServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Không tìm thấy connection string 'DefaultConnection' trong appsettings.json.");

        services.AddScoped<IAuditContext, AuditContext>();
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<GiaPhaDbContext>((serviceProvider, options) =>
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(GiaPhaDbContext).Assembly.FullName))
            .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));

        services.AddScoped(typeof(IGenericDao<>), typeof(GenericDao<>));
        services.AddScoped<IClanDao, ClanDao>();
        services.AddScoped<GenealogySeeder>();

        return services;
    }
}
