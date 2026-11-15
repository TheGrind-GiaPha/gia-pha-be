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
        services.AddScoped<IImportBatchRepository, ImportBatchRepository>();
        services.AddScoped<IImportRowRepository, ImportRowRepository>();
        services.AddScoped<IExportJobRepository, ExportJobRepository>();
        services.AddScoped<IChangeRequestRepository, ChangeRequestRepository>();
        services.AddScoped<IRepositoryManager, RepositoryManager>();
        return services;
    }
}
