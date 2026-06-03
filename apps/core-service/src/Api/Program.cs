using DeskMatch.BuildingBlocks.Extensions;
using DeskMatch.SDK.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseBuildingBlocks(builder.Configuration);

builder.Services.AddBuildingBlocks(builder.Configuration);

// ─────────────────────────────── Application DI ──
// TODO: Move registrations to DependencyInjection.cs once implemented
// builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddStorageSdk(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseBuildingBlocksMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();