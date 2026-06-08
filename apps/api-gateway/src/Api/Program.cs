using DeskMatch.ApiGateway.Api.Extensions;
using DeskMatch.ApiGateway.Api.Middleware;
using DeskMatch.BuildingBlocks.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddBuildingBlockConfiguration();

builder.Host.UseBuildingBlocks(builder.Configuration);

builder.Services.AddBuildingBlocks(builder.Configuration);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var corsOrigins = builder.Configuration.GetValue<string>("CORS_ALLOWED_ORIGINS");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (!string.IsNullOrWhiteSpace(corsOrigins))
        {
            policy.WithOrigins(corsOrigins.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors();

app.UseBuildingBlocksMiddleware();

app.UseMiddleware<CorrelationIdPropagationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.MapHealthChecks("/health");

app.UseSwaggerDocs();

app.Run();
