using DeskMatch.BuildingBlocks.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddBuildingBlockConfiguration();

builder.Host.UseBuildingBlocks(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddBuildingBlocks(builder.Configuration);

// ─────────────────────────────── Application DI ──
// TODO: Move registrations to DependencyInjection.cs once implemented
// builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseBuildingBlocksMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();