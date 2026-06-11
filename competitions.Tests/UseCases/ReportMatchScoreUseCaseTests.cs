using Xunit;
using competitions.Application;
using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Matches.Models;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Domain.Models;
using competitions.Shared;
using NSubstitute;
using Engine = Revyne.Engine.Api;

namespace competitions.Tests.UseCases;

public class ReportMatchScoreUseCaseTests
{
    private readonly IMatchRepository _matchRepo = Substitute.For<IMatchRepository>();
    private readonly Engine.ICompetitionEngine _engine = Substitute.For<Engine.ICompetitionEngine>();

    private ReportMatchScoreUseCase CreateSut() => new(_matchRepo, _engine);

    private static Engine.Result<Engine.MatchOutcome, Engine.CompetitionError> Outcome(
        string? winner = "teamA", string? loser = "teamB") =>
        Engine.Result<Engine.MatchOutcome, Engine.CompetitionError>.Success(
            new Engine.MatchOutcome { WinnerTeamId = winner, LoserTeamId = loser });

    private static Engine.Result<Engine.MatchOutcome, Engine.CompetitionError> EngineError(
        Engine.CompetitionError error) =>
        Engine.Result<Engine.MatchOutcome, Engine.CompetitionError>.Failure(error);

    private void StubEngine(Engine.Result<Engine.MatchOutcome, Engine.CompetitionError> result) =>
        _engine.ResolveOutcome(Arg.Any<Engine.EngineCompetitionConfig>(), Arg.Any<Engine.FinishedMatch>())
            .Returns(result);

    private static Match MatchWithLeague(string id = "match1") => new()
    {
        Id = id,
        CompetitionId = "league1",
        HomeTeamId = "teamA",
        AwayTeamId = "teamB",
        TenantId = "tenant1",
        Meta = new MatchMeta(),
        Competition = new League
        {
            Id = "league1",
            Name = "League",
            TenantId = "tenant1",
        },
    };

    private static Match MatchWithTournament(string id = "match1") => new()
    {
        Id = id,
        CompetitionId = "tournament1",
        HomeTeamId = "teamA",
        AwayTeamId = "teamB",
        TenantId = "tenant1",
        Meta = new MatchMeta(),
        Competition = new Tournament
        {
            Id = "tournament1",
            Name = "Tourney",
            TenantId = "tenant1",
            Format = TournamentFormat.SingleElimination,
        },
    };

    private static Match MatchWithoutCompetition(string id = "match1") => new()
    {
        Id = id,
        CompetitionId = "comp1",
        HomeTeamId = "teamA",
        AwayTeamId = "teamB",
        TenantId = "tenant1",
        Meta = new MatchMeta(),
        Competition = null,
    };

    [Fact]
    public async Task Execute_ReturnsUnit_WhenLeagueMatchScoreReported()
    {
        var match = MatchWithLeague();
        _matchRepo.GetByIdAsync("match1", "tenant1")
            .Returns(Result<Match, RepositoryError>.Success(match));
        StubEngine(Outcome());
        _matchRepo.Update(Arg.Any<Match>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("match1", 3, 1, "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Equal(3, match.ScoreHome);
        Assert.Equal(1, match.ScoreAway);
        Assert.Equal("teamA", match.WinnerTeamId);
        Assert.Equal("teamB", match.LoserTeamId);
        _engine.Received(1).ResolveOutcome(Arg.Any<Engine.EngineCompetitionConfig>(), Arg.Any<Engine.FinishedMatch>());
        await _matchRepo.Received(1).Update(match);
    }

    [Fact]
    public async Task Execute_ReturnsUnit_WhenTournamentMatchScoreReported()
    {
        var match = MatchWithTournament();
        _matchRepo.GetByIdAsync("match1", "tenant1")
            .Returns(Result<Match, RepositoryError>.Success(match));
        StubEngine(Outcome());
        _matchRepo.Update(Arg.Any<Match>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("match1", 2, 0, "tenant1");

        Assert.True(result.IsSuccess);
        _engine.Received(1).ResolveOutcome(Arg.Any<Engine.EngineCompetitionConfig>(), Arg.Any<Engine.FinishedMatch>());
    }

    [Fact]
    public async Task Execute_ReturnsNotFound_WhenMatchDoesNotExist()
    {
        _matchRepo.GetByIdAsync("match1", "tenant1")
            .Returns(Result<Match, RepositoryError>.Failure(RepositoryError.NotFound));

        var result = await CreateSut().Execute("match1", 1, 0, "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.NotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenMatchFetchFails()
    {
        _matchRepo.GetByIdAsync("match1", "tenant1")
            .Returns(Result<Match, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("match1", 1, 0, "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenCompetitionIsNull()
    {
        _matchRepo.GetByIdAsync("match1", "tenant1")
            .Returns(Result<Match, RepositoryError>.Success(MatchWithoutCompetition()));

        var result = await CreateSut().Execute("match1", 1, 0, "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenEngineReturnsInvalidArguments()
    {
        var match = MatchWithLeague();
        _matchRepo.GetByIdAsync("match1", "tenant1")
            .Returns(Result<Match, RepositoryError>.Success(match));
        StubEngine(EngineError(Engine.CompetitionError.InvalidArguments));

        var result = await CreateSut().Execute("match1", 1, 0, "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsNotFound_WhenEngineReturnsNotFound()
    {
        var match = MatchWithLeague();
        _matchRepo.GetByIdAsync("match1", "tenant1")
            .Returns(Result<Match, RepositoryError>.Success(match));
        StubEngine(EngineError(Engine.CompetitionError.NotFound));

        var result = await CreateSut().Execute("match1", 1, 0, "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.NotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenEngineReturnsInternalError()
    {
        var match = MatchWithLeague();
        _matchRepo.GetByIdAsync("match1", "tenant1")
            .Returns(Result<Match, RepositoryError>.Success(match));
        StubEngine(EngineError(Engine.CompetitionError.InternalError));

        var result = await CreateSut().Execute("match1", 1, 0, "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenUpdateFails()
    {
        var match = MatchWithLeague();
        _matchRepo.GetByIdAsync("match1", "tenant1")
            .Returns(Result<Match, RepositoryError>.Success(match));
        StubEngine(Outcome());
        _matchRepo.Update(Arg.Any<Match>())
            .Returns(Result<Unit, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("match1", 1, 0, "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_SetsScoresBeforeCallingEngine()
    {
        var match = MatchWithLeague();
        _matchRepo.GetByIdAsync("match1", "tenant1")
            .Returns(Result<Match, RepositoryError>.Success(match));
        _engine.ResolveOutcome(Arg.Any<Engine.EngineCompetitionConfig>(), Arg.Do<Engine.FinishedMatch>(fm =>
        {
            Assert.Equal(5, fm.ScoreHome);
            Assert.Equal(2, fm.ScoreAway);
        })).Returns(Outcome());
        _matchRepo.Update(Arg.Any<Match>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("match1", 5, 2, "tenant1");

        Assert.True(result.IsSuccess);
    }
}
