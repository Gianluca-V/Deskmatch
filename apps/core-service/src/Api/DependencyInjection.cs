// Register your application services here.
// Called from Program.cs via: builder.Services.AddApplicationServices(builder.Configuration);

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeskMatch.CoreService.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Add DbContext (Npgsql)
        // services.AddDbContext<CoreDbContext>(options =>
        //     options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        // services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CoreDbContext>());

        // TODO: Add SDKs
        // services.AddOpenSearchSdk(configuration);
        // services.AddRedisSdk(configuration);
        // services.AddNotificationSdk(configuration);

        // TODO: Register repositories
        // services.AddScoped<IOfficeRepository, OfficeRepository>();
        // services.AddScoped<ICompanyRepository, CompanyRepository>();
        // services.AddScoped<IReservationRepository, ReservationRepository>();
        // services.AddScoped<IReviewRepository, ReviewRepository>();
        // services.AddScoped<IPaymentRepository, PaymentRepository>();

        // TODO: Register domain services (indexers, etc.)
        // services.AddScoped<OpenSearchOfficeIndexer>();

        // TODO: Register command handlers (Offices)
        // services.AddScoped<ICommandHandler<CreateOfficeCommand, Guid>, CreateOfficeCommandHandler>();
        // services.AddScoped<ICommandHandler<UpdateOfficeCommand>, UpdateOfficeCommandHandler>();
        // services.AddScoped<ICommandHandler<DeleteOfficeCommand>, DeleteOfficeCommandHandler>();

        // TODO: Register command handlers (Companies)
        // services.AddScoped<ICommandHandler<CreateCompanyCommand, Guid>, CreateCompanyCommandHandler>();
        // services.AddScoped<ICommandHandler<UpdateCompanyCommand>, UpdateCompanyCommandHandler>();

        // TODO: Register command handlers (Reservations)
        // services.AddScoped<ICommandHandler<CreateReservationCommand, Guid>, CreateReservationCommandHandler>();
        // services.AddScoped<ICommandHandler<UpdateReservationStatusCommand>, UpdateReservationStatusCommandHandler>();

        // TODO: Register command handlers (Payments)
        // services.AddScoped<ICommandHandler<ProcessDepositCommand, Guid>, ProcessDepositCommandHandler>();
        // services.AddScoped<ICommandHandler<CreateFinalPaymentCommand, Guid>, CreateFinalPaymentCommandHandler>();
        // services.AddScoped<ICommandHandler<ProcessFinalPaymentCommand, Guid>, ProcessFinalPaymentCommandHandler>();

        // TODO: Register command handlers (Reviews)
        // services.AddScoped<ICommandHandler<CreateReviewCommand, Guid>, CreateReviewCommandHandler>();

        // TODO: Register query handlers
        // services.AddScoped<IQueryHandler<GetOfficeByIdQuery, OfficeDto?>, GetOfficeByIdQueryHandler>();
        // services.AddScoped<IQueryHandler<GetAllOfficesQuery, GetAllOfficesResult>, GetAllOfficesQueryHandler>();
        // services.AddScoped<IQueryHandler<GetCompanyByIdQuery, CompanyDto?>, GetCompanyByIdQueryHandler>();
        // services.AddScoped<IQueryHandler<GetReservationByIdQuery, ReservationDto?>, GetReservationByIdQueryHandler>();
        // services.AddScoped<IQueryHandler<GetReviewsByOfficeQuery, IReadOnlyList<ReviewDto>>, GetReviewsByOfficeQueryHandler>();
        // services.AddScoped<IQueryHandler<GetPaymentsByReservationQuery, IReadOnlyList<PaymentDto>>, GetPaymentsByReservationQueryHandler>();

        // TODO: Register FluentValidation validators for all commands

        return services;
    }
}