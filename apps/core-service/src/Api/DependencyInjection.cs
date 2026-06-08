
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DeskMatch.CoreService.Application.Companies.Commands;
using DeskMatch.CoreService.Application.Companies.Handlers;
using DeskMatch.CoreService.Application.Companies.Interfaces;
using DeskMatch.CoreService.Infrastructure.Persistence;
using DeskMatch.CoreService.Infrastructure.Repositories;
using DeskMatch.Domain.CQRS;
using DeskMatch.SDK.Geocoding;
using DeskMatch.SDK.Storage;
using DeskMatch.SDK.OpenSearch;
using DeskMatch.SDK.Ollama;
using DeskMatch.SDK.OpenSearch.Documents;
using OpenSearch.Client;
using Microsoft.EntityFrameworkCore;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.CoreService.Application.Workspaces.Commands;
using DeskMatch.CoreService.Application.Workspaces.Handlers;

namespace DeskMatch.CoreService.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CoreDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICommandHandler<CreateCompanyCommand, Guid>, CreateCompanyCommandHandler>();

        services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        services.AddScoped<ICommandHandler<CreateWorkspaceCommand, Guid>, CreateWorkspaceCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateWorkspaceCommand>, UpdateWorkspaceCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteWorkspaceCommand>, DeleteWorkspaceCommandHandler>();

        services.AddGeocodingSdk(configuration);
        services.AddStorageSdk(configuration);
        services.AddOpenSearchSdk(configuration);
        services.AddOllamaClient();

        services.AddSingleton(provider =>
        {
            var client = provider.GetRequiredService<IOpenSearchClient>();
            return new OpenSearchRepository<WorkspaceDocument>(client);
        });

        return services;
    }
}
