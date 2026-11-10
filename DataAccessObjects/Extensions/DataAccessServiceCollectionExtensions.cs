using GiaPha.DataAccessObjects.Context;
using GiaPha.DataAccessObjects.DAOs;
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

        services.AddDbContext<GiaPhaDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(GiaPhaDbContext).Assembly.FullName)));

        services.AddScoped(typeof(IGenericDao<>), typeof(GenericDao<>));
        services.AddScoped<IClanDao, ClanDao>();

        return services;
    }
}
