using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Leagues;
using competitions.Domain.Competitions.Shared.Models;
using competitions.Domain.Competitions.Tournaments;
using competitions.Infrastructure.Helpers;
using competitions.Infrastructure.Plugins;
using competitions.Infrastructure.Repositories;
using competitions.Infrastructure.Services;
using competitions.Services;
using competitions.Application.Engines;
using competitions.Transport.Services;
using Microsoft.EntityFrameworkCore;
using Revyne.Engine.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (databaseUrl is null)
{
    Environment.FailFast("DATABASE_URL environment variable is required.");
}

builder.Services.AddDbContextFactory<DatabaseService>(options =>
    options.UseNpgsql(databaseUrl));
builder.Services.AddSingleton<IIDGenerator, NanoIdGenerator>();
builder.Services.AddSingleton<IDiscriminatorGenerator, NanoIdGenerator>();
builder.Services.AddSingleton<IPermissionService, KetoPermissionService>((args) =>
{
    var ketoUrl = Environment.GetEnvironmentVariable("KETO_URL");
    var ketoAdminUrl = Environment.GetEnvironmentVariable("KETO_ADMIN_URL");
    if (ketoUrl is null)
    {
        Console.WriteLine("Missing KETO_URL");
        Environment.Exit(1);
    }

    if (ketoAdminUrl is null)
    {
        Console.WriteLine("Missing KETO_ADMIN_URL");
        Environment.Exit(1);
    }
    
    var logger = args.GetRequiredService<ILogger<KetoPermissionService>>();
    
    return new KetoPermissionService(logger, ketoUrl, ketoAdminUrl);
});

builder.Services.AddScoped<ITournamentRepository, TournamentRepository>();
builder.Services.AddScoped<ILeagueRepository, LeagueRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();

// Competition engines. At startup we scan a plugins folder for DLLs and discover
// every ICompetitionEngine inside them; those are registered ahead of the open
// baseline so a plugin can override the reference engine for keys it claims. At
// runtime DispatchingCompetitionEngine — the single engine the use cases inject —
// routes each competition to the first engine whose CanHandle() matches the
// config's EngineKey, falling back to the reference engine.
using (var startupLoggerFactory = LoggerFactory.Create(logging => logging.AddConsole()))
{
    var pluginLogger = startupLoggerFactory.CreateLogger("competitions.Plugins");
    var pluginsPath = Environment.GetEnvironmentVariable("PLUGINS_PATH")
                      ?? Path.Combine(AppContext.BaseDirectory, "plugins");

    var engineTypes = new List<Type>();
    engineTypes.AddRange(PluginLoader.DiscoverEngineTypes(pluginsPath, pluginLogger));
    engineTypes.Add(typeof(ReferenceCompetitionEngine)); // fallback, always last

    foreach (var engineType in engineTypes)
        builder.Services.AddScoped(engineType);

    builder.Services.AddSingleton(new CompetitionEngineCatalog(engineTypes));
}

builder.Services.AddScoped<ICompetitionEngine, DispatchingCompetitionEngine>();

builder.Services.AddScoped<CreateTournamentUseCase>();
builder.Services.AddScoped<RegisterTeamForTournamentUseCase>();
builder.Services.AddScoped<CreateLeagueUseCase>();
builder.Services.AddScoped<CreateDivisionUseCase>();
builder.Services.AddScoped<CreateDivisionGroupUseCase>();
builder.Services.AddScoped<AddTeamToLeagueUseCase>();
builder.Services.AddScoped<RegisterTeamForLeagueUseCase>();
builder.Services.AddScoped<UnregisterTeamFromLeagueUseCase>();
builder.Services.AddScoped<ReportMatchScoreUseCase>();
builder.Services.AddScoped<CreateMatchesForLeagueUseCase>();
builder.Services.AddScoped<CreateMatchesForTournamentUseCase>();
builder.Services.AddScoped<CreateNextRoundForTournamentUseCase>();
builder.Services.AddScoped<EditLeagueUseCase>();
builder.Services.AddScoped<EditTournamentUseCase>();
builder.Services.AddScoped<DeleteLeagueUseCase>();
builder.Services.AddScoped<DeleteTournamentUseCase>();
builder.Services.AddScoped<JoinLeagueUseCase>();
builder.Services.AddScoped<UpdateTournamentRulesUseCase>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<MatchesService>();
app.MapGrpcService<CompetitionsService>();
app.MapGrpcService<LeaguesService>();

app.Run();