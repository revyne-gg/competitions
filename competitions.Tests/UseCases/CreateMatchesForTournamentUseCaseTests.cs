using Xunit;
using competitions.Application;
using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Matches.Models;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Shared;
using NSubstitute;

namespace competitions.Tests.UseCases;

public class CreateMatchesForTournamentUseCaseTests
{
    private readonly ITournamentRepository _tournamentRepo = Substitute.For<ITournamentRepository>();
    private readonly IMatchRepository _matchRepo = Substitute.For<IMatchRepository>();
    private readonly IIDGenerator _idGenerator = Substitute.For<IIDGenerator>();

    private CreateMatchesForTournamentUseCase CreateSut() => new(_tournamentRepo, _matchRepo, _idGenerator);

    private static Tournament DefaultTournament(TournamentFormat format = TournamentFormat.SingleElimination) => new()
    {
        Id = "tournament1",
        Name = "Tourney",
        TenantId = "tenant1",
        Format = format,
    };

    private int _idCounter;
    private void SetupIdGenerator()
    {
        _idCounter = 0;
        _idGenerator.Generate().Returns(_ => Task.FromResult($"id{++_idCounter}"));
    }

    [Fact]
    public async Task Execute_ReturnsMatches_ForFourTeamsSingleElimination()
    {
        SetupIdGenerator();
        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament()));
        _matchRepo.GetByCompetitionIdAsync("tournament1", "tenant1")
            .Returns(Result<List<Match>, RepositoryError>.Success(new List<Match>()));
        _matchRepo.AddRangeAsync(Arg.Any<List<Match>>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var teams = new List<string> { "t1", "t2", "t3", "t4" };
        var result = await CreateSut().Execute("tournament1", teams, "tenant1");

        Assert.True(result.IsSuccess);
        // Standard bracket seeding: t1 vs t4, t2 vs t3
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal("t1", result.Value[0].HomeTeamId);
        Assert.Equal("t4", result.Value[0].AwayTeamId);
        Assert.Equal("t2", result.Value[1].HomeTeamId);
        Assert.Equal("t3", result.Value[1].AwayTeamId);
        Assert.All(result.Value, m => Assert.Equal(1, m.Round));
        Assert.All(result.Value, m => Assert.Equal("tournament1", m.CompetitionId));
        Assert.All(result.Value, m => Assert.StartsWith("match_", m.Id));
    }

    [Fact]
    public async Task Execute_ReturnsMatches_ForDoubleElimination()
    {
        SetupIdGenerator();
        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament(TournamentFormat.DoubleElimination)));
        _matchRepo.GetByCompetitionIdAsync("tournament1", "tenant1")
            .Returns(Result<List<Match>, RepositoryError>.Success(new List<Match>()));
        _matchRepo.AddRangeAsync(Arg.Any<List<Match>>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var teams = new List<string> { "t1", "t2" };
        var result = await CreateSut().Execute("tournament1", teams, "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenFewerThanTwoTeams()
    {
        var result = await CreateSut().Execute("tournament1", new List<string> { "t1" }, "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenEmptyTeamList()
    {
        var result = await CreateSut().Execute("tournament1", new List<string>(), "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsNotFound_WhenTournamentDoesNotExist()
    {
        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Failure(RepositoryError.NotFound));

        var result = await CreateSut().Execute("tournament1", new List<string> { "t1", "t2" }, "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.NotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenTournamentFetchFails()
    {
        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("tournament1", new List<string> { "t1", "t2" }, "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenFormatIsSwiss()
    {
        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament(TournamentFormat.Swiss)));

        var result = await CreateSut().Execute("tournament1", new List<string> { "t1", "t2" }, "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenFormatIsRoundRobin()
    {
        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament(TournamentFormat.RoundRobin)));

        var result = await CreateSut().Execute("tournament1", new List<string> { "t1", "t2" }, "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsConflict_WhenMatchesAlreadyExist()
    {
        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament()));
        _matchRepo.GetByCompetitionIdAsync("tournament1", "tenant1")
            .Returns(Result<List<Match>, RepositoryError>.Success(new List<Match>
            {
                new() { Id = "match_existing", CompetitionId = "tournament1", TenantId = "tenant1", Meta = new MatchMeta() },
            }));

        var result = await CreateSut().Execute("tournament1", new List<string> { "t1", "t2" }, "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.Conflict, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenExistingMatchesFetchFails()
    {
        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament()));
        _matchRepo.GetByCompetitionIdAsync("tournament1", "tenant1")
            .Returns(Result<List<Match>, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("tournament1", new List<string> { "t1", "t2" }, "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenSaveFails()
    {
        SetupIdGenerator();
        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament()));
        _matchRepo.GetByCompetitionIdAsync("tournament1", "tenant1")
            .Returns(Result<List<Match>, RepositoryError>.Success(new List<Match>()));
        _matchRepo.AddRangeAsync(Arg.Any<List<Match>>())
            .Returns(Result<Unit, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("tournament1", new List<string> { "t1", "t2" }, "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_SeedsBracketCorrectly_ForSixTeams()
    {
        SetupIdGenerator();
        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament()));
        _matchRepo.GetByCompetitionIdAsync("tournament1", "tenant1")
            .Returns(Result<List<Match>, RepositoryError>.Success(new List<Match>()));
        _matchRepo.AddRangeAsync(Arg.Any<List<Match>>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var teams = new List<string> { "s1", "s2", "s3", "s4", "s5", "s6" };
        var result = await CreateSut().Execute("tournament1", teams, "tenant1");

        Assert.True(result.IsSuccess);
        // 6/2 = 3 pairs: s1 vs s6, s2 vs s5, s3 vs s4
        Assert.Equal(3, result.Value!.Count);
        Assert.Equal("s1", result.Value[0].HomeTeamId);
        Assert.Equal("s6", result.Value[0].AwayTeamId);
        Assert.Equal("s2", result.Value[1].HomeTeamId);
        Assert.Equal("s5", result.Value[1].AwayTeamId);
        Assert.Equal("s3", result.Value[2].HomeTeamId);
        Assert.Equal("s4", result.Value[2].AwayTeamId);
    }
}
