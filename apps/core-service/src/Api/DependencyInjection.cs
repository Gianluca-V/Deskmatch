using DeskMatch.CoreService.Application.Companies.Commands;
using DeskMatch.CoreService.Application.Companies.Dtos;
using DeskMatch.CoreService.Application.Companies.Handlers;
using DeskMatch.CoreService.Application.Companies.Interfaces;
using DeskMatch.CoreService.Application.Companies.Validators;
using DeskMatch.CoreService.Application.Dashboard.Interfaces;
using DeskMatch.CoreService.Application.Dashboard.Services;
using DeskMatch.CoreService.Application.Reservations.Commands;
using DeskMatch.CoreService.Application.Reservations.Handlers;
using DeskMatch.CoreService.Application.Reservations.Interfaces;
using DeskMatch.CoreService.Application.Workspaces.Commands;
using DeskMatch.CoreService.Application.Workspaces.Handlers;
using DeskMatch.CoreService.Application.Workspaces.Interfaces;
using DeskMatch.CoreService.Application.Workspaces.Models;
using DeskMatch.CoreService.Application.Workspaces.Services;
using DeskMatch.CoreService.Infrastructure.Persistence;
using DeskMatch.CoreService.Infrastructure.Repositories;
using DeskMatch.Domain.CQRS;
using DeskMatch.SDK.Geocoding;
using DeskMatch.SDK.Ollama;
using DeskMatch.SDK.OpenSearch;
using DeskMatch.SDK.Storage;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeskMatch.CoreService.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CoreDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICommandHandler<CreateCompanyCommand, Guid>, CreateCompanyCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateCompanyProfileCommand, CompanyProfileResponseDto>, UpdateCompanyProfileCommandHandler>();

        services.AddValidatorsFromAssemblyContaining<CompanyUpdateProfileDtoValidator>();

        services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
        services.AddScoped<ICommandHandler<CreateWorkspaceCommand, Guid>, CreateWorkspaceCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateWorkspaceCommand>, UpdateWorkspaceCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteWorkspaceCommand>, DeleteWorkspaceCommandHandler>();
        services.AddScoped<ICommandHandler<ReindexWorkspacesCommand>, ReindexWorkspacesCommandHandler>();
        services.AddScoped<ICommandHandler<BulkConfirmCommand, BulkCreateResponse>, BulkCreateWorkspaceCommandHandler>();

        services.AddScoped<IExcelTemplateService, ExcelTemplateService>();
        services.AddScoped<IExcelWorkspaceParser, ExcelWorkspaceParser>();
        services.AddScoped<IWorkspacePreviewService, WorkspacePreviewService>();

        services.AddOpenSearchSdk(configuration);
        services.AddOllamaClient();

        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<ICommandHandler<CreateReservationCommand, Guid>, CreateReservationCommandHandler>();
        services.AddScoped<ICommandHandler<CancelReservationCommand>, CancelReservationCommandHandler>();

        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IDashboardService, DashboardService>();

        services.AddGeocodingSdk(configuration);
        services.AddStorageSdk(configuration);

        return services;
    }
}