using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Leagues;
using competitions.Domain.Competitions.Shared.Models;
using competitions.Domain.Competitions.Tournaments;
using competitions.Infrastructure.Helpers;
using competitions.Infrastructure.Repositories;
using competitions.Infrastructure.Services;
using competitions.Services;
using competitions.Transport.Services;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddScoped<ICompetitionEngine, TournamentEngine>();
builder.Services.AddScoped<ITournamentEngine, SingleEliminationEngine>();
builder.Services.AddScoped<ITournamentEngine, DoubleEliminationEngine>();
builder.Services.AddScoped<ITournamentEngine, SwissEngine>();
builder.Services.AddScoped<ITournamentEngine, RoundRobinEngine>();
builder.Services.AddScoped<ICompetitionEngine, LeagueEngine>();

builder.Services.AddScoped<CreateTournamentUseCase>();
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

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<MatchesService>();
app.MapGrpcService<CompetitionsService>();
app.MapGrpcService<LeaguesService>();

app.Run();