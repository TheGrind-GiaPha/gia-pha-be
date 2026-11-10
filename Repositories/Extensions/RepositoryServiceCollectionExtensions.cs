using GiaPha.Repositories.Implementations;
using GiaPha.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GiaPha.Repositories.Extensions;

public static class RepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IClanRepository, ClanRepository>();
        services.AddScoped<IRepositoryManager, RepositoryManager>();
        return services;
    }
}
