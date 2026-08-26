using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sergin.SharedKernel.Infrastructure.Data.EFCore;
using Sergin.SharedKernel.Modules;
using Sergin.UserAccess.Application;
using Sergin.UserAccess.Application.Contracts;
using Sergin.UserAccess.Infrastructure.Data;
using Sergin.UserAccess.Presentation.Blazor;
using Sergin.UserAccess.Roles;
using Sergin.UserAccess.Users;

namespace Sergin.UserAccess;

public sealed class UserAccessModule : ISerginWebApiModule, ISerginWebUiModule
{
    public string Schema => UserAccessDbContext.Schema;

    public Assembly ApplicationAssembly => UserAccessApplicationAssemblyReference.Assembly;

    public Assembly ContractsAssembly => UserAccessApplicationContractsAssemblyReference.Assembly;

    public Assembly UiAssembly => UserAccessBlazorAssemblyReference.Assembly;

    public IReadOnlyCollection<SerginNavItem> NavItems => UserAccessNavigation.Items;

    public void AddServices(IServiceCollection services, IConfigurationSection configuration)
    {
        services.AddModuleDbContext<UserAccessDbContext, IUserAccessDbContext, IUserAccessUnitOfWork>(configuration, UserAccessDbContext.Schema);

        services.AddUserDependencies();

        services.AddRoleDependencies();
    }

    public Task MigrateAsync(IServiceProvider services) => services.MigrateDbContextAsync<UserAccessDbContext>();

    public void MapEndpoints(RouteGroupBuilder group) => group.MapUserEndpoints();
}
