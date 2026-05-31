

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DeskMatch.CoreService.Application.Companies.Commands;
using DeskMatch.CoreService.Application.Companies.Handlers;
using DeskMatch.CoreService.Application.Companies.Interfaces;
using DeskMatch.CoreService.Infrastructure.Persistence;
using DeskMatch.CoreService.Infrastructure.Repositories;
using DeskMatch.Domain.CQRS;
using Microsoft.EntityFrameworkCore;

namespace DeskMatch.CoreService.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CoreDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICommandHandler<CreateCompanyCommand, Guid>, CreateCompanyCommandHandler>();
        return services;
    }
}