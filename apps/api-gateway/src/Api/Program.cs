using DeskMatch.ApiGateway.Api.Extensions;
using DeskMatch.ApiGateway.Api.Middleware;
using DeskMatch.BuildingBlocks.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseBuildingBlocks(builder.Configuration);

builder.Services.AddBuildingBlocks(builder.Configuration);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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
