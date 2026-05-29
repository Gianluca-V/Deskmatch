using DeskMatch.BuildingBlocks.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseBuildingBlocks(builder.Configuration);

builder.Services.AddBuildingBlocks(builder.Configuration);

// ─────────────────────────────── Application DI ──
// TODO: Move registrations to DependencyInjection.cs once implemented
// builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseBuildingBlocksMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();