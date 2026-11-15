using GiaPha.Repositories.Implementations;
using GiaPha.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GiaPha.Repositories.Extensions;

public static class RepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IClanRepository, ClanRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserClanRoleRepository, UserClanRoleRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IParentChildRepository, ParentChildRepository>();
        services.AddScoped<IUnionRepository, UnionRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IRepositoryManager, RepositoryManager>();
        return services;
    }
}
