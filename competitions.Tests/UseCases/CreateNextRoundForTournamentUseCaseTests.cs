using Xunit;
using competitions.Application;
using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Matches.Models;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Shared;
using NSubstitute;

namespace competitions.Tests.UseCases;

public class CreateNextRoundForTournamentUseCaseTests
{
    private readonly ITournamentRepository _tournamentRepo = Substitute.For<ITournamentRepository>();
    private readonly IMatchRepository _matchRepo = Substitute.For<IMatchRepository>();
    private readonly IIDGenerator _idGenerator = Substitute.For<IIDGenerator>();

    private CreateNextRoundForTournamentUseCase CreateSut() => new(_tournamentRepo, _matchRepo, _idGenerator);

    private static Tournament DefaultTournament(TournamentFormat format = TournamentFormat.SingleElimination) => new()
    {
        Id = "tournament1",
        Name = "Tourney",
        TenantId = "tenant1",
        Format = format,
    };

    private static Match FinishedMatch(string id, int round, string home, string away, string winner, DateTime createdAt) => new()
    {
        Id = id,
        CompetitionId = "tournament1",
        HomeTeamId = home,
        AwayTeamId = away,
        WinnerTeamId = winner,
        LoserTeamId = winner == home ? away : home,
        Round = round,
        TenantId = "tenant1",
        CreatedAt = createdAt,
        Meta = new MatchMeta(),
    };

    private static Match UnfinishedMatch(string id, int round, string home, string away, DateTime createdAt) => new()
    {
        Id = id,
        CompetitionId = "tournament1",
        HomeTeamId = home,
        AwayTeamId = away,
        Round = round,
        TenantId = "tenant1",
        CreatedAt = createdAt,
        Meta = new MatchMeta(),
    };

    private int _idCounter;
    private void SetupIdGenerator()
    {
        _idCounter = 0;
        _idGenerator.Generate().Returns(_ => Task.FromResult($"id{++_idCounter}"));
    }

    [Fact]
    public async Task Execute_ReturnsNextRoundMatches_WhenAllCurrentRoundFinished()
    {
        SetupIdGenerator();
        var now = DateTime.UtcNow;
        var matches = new List<Match>
        {
            FinishedMatch("m1", 1, "t1", "t4", "t1", now),
            FinishedMatch("m2", 1, "t2", "t3", "t3", now.AddSeconds(1)),
        };

        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament()));
        _matchRepo.GetByCompetitionIdAsync("tournament1", "tenant1")
            .Returns(Result<List<Match>, RepositoryError>.Success(matches));
        _matchRepo.AddRangeAsync(Arg.Any<List<Match>>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("tournament1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        // Winners in creation order: t1, t3
        Assert.Equal("t1", result.Value[0].HomeTeamId);
        Assert.Equal("t3", result.Value[0].AwayTeamId);
        Assert.Equal(2, result.Value[0].Round);
        Assert.Equal("tournament1", result.Value[0].CompetitionId);
        Assert.StartsWith("match_", result.Value[0].Id);
    }

    [Fact]
    public async Task Execute_AdvancesFromRound2ToRound3()
    {
        SetupIdGenerator();
        var now = DateTime.UtcNow;
        var matches = new List<Match>
        {
            // Round 1
            FinishedMatch("m1", 1, "t1", "t8", "t1", now),
            FinishedMatch("m2", 1, "t2", "t7", "t2", now.AddSeconds(1)),
            FinishedMatch("m3", 1, "t3", "t6", "t3", now.AddSeconds(2)),
            FinishedMatch("m4", 1, "t4", "t5", "t4", now.AddSeconds(3)),
            // Round 2
            FinishedMatch("m5", 2, "t1", "t2", "t1", now.AddSeconds(4)),
            FinishedMatch("m6", 2, "t3", "t4", "t4", now.AddSeconds(5)),
        };

        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament()));
        _matchRepo.GetByCompetitionIdAsync("tournament1", "tenant1")
            .Returns(Result<List<Match>, RepositoryError>.Success(matches));
        _matchRepo.AddRangeAsync(Arg.Any<List<Match>>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("tournament1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal(3, result.Value[0].Round);
        Assert.Equal("t1", result.Value[0].HomeTeamId);
        Assert.Equal("t4", result.Value[0].AwayTeamId);
    }

    [Fact]
    public async Task Execute_ReturnsNotFound_WhenTournamentDoesNotExist()
    {
        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Failure(RepositoryError.NotFound));

        var result = await CreateSut().Execute("tournament1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.NotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenTournamentFetchFails()
    {
        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("tournament1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenFormatIsSwiss()
    {
        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament(TournamentFormat.Swiss)));

        var result = await CreateSut().Execute("tournament1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenFormatIsRoundRobin()
    {
        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament(TournamentFormat.RoundRobin)));

        var result = await CreateSut().Execute("tournament1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenNoMatchesExist()
    {
        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament()));
        _matchRepo.GetByCompetitionIdAsync("tournament1", "tenant1")
            .Returns(Result<List<Match>, RepositoryError>.Success(new List<Match>()));

        var result = await CreateSut().Execute("tournament1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsConflict_WhenCurrentRoundStillInProgress()
    {
        var now = DateTime.UtcNow;
        var matches = new List<Match>
        {
            FinishedMatch("m1", 1, "t1", "t4", "t1", now),
            UnfinishedMatch("m2", 1, "t2", "t3", now.AddSeconds(1)),
        };

        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament()));
        _matchRepo.GetByCompetitionIdAsync("tournament1", "tenant1")
            .Returns(Result<List<Match>, RepositoryError>.Success(matches));

        var result = await CreateSut().Execute("tournament1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.Conflict, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsConflict_WhenTournamentComplete_OneWinnerRemains()
    {
        var now = DateTime.UtcNow;
        var matches = new List<Match>
        {
            FinishedMatch("m1", 1, "t1", "t2", "t1", now),
        };

        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament()));
        _matchRepo.GetByCompetitionIdAsync("tournament1", "tenant1")
            .Returns(Result<List<Match>, RepositoryError>.Success(matches));

        var result = await CreateSut().Execute("tournament1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.Conflict, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenOddNumberOfWinners()
    {
        var now = DateTime.UtcNow;
        var matches = new List<Match>
        {
            FinishedMatch("m1", 1, "t1", "t6", "t1", now),
            FinishedMatch("m2", 1, "t2", "t5", "t2", now.AddSeconds(1)),
            FinishedMatch("m3", 1, "t3", "t4", "t3", now.AddSeconds(2)),
        };

        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament()));
        _matchRepo.GetByCompetitionIdAsync("tournament1", "tenant1")
            .Returns(Result<List<Match>, RepositoryError>.Success(matches));

        var result = await CreateSut().Execute("tournament1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenMatchesFetchFails()
    {
        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament()));
        _matchRepo.GetByCompetitionIdAsync("tournament1", "tenant1")
            .Returns(Result<List<Match>, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("tournament1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenSaveFails()
    {
        SetupIdGenerator();
        var now = DateTime.UtcNow;
        var matches = new List<Match>
        {
            FinishedMatch("m1", 1, "t1", "t4", "t1", now),
            FinishedMatch("m2", 1, "t2", "t3", "t3", now.AddSeconds(1)),
        };

        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament()));
        _matchRepo.GetByCompetitionIdAsync("tournament1", "tenant1")
            .Returns(Result<List<Match>, RepositoryError>.Success(matches));
        _matchRepo.AddRangeAsync(Arg.Any<List<Match>>())
            .Returns(Result<Unit, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("tournament1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_WorksWithDoubleElimination()
    {
        SetupIdGenerator();
        var now = DateTime.UtcNow;
        var matches = new List<Match>
        {
            FinishedMatch("m1", 1, "t1", "t4", "t1", now),
            FinishedMatch("m2", 1, "t2", "t3", "t2", now.AddSeconds(1)),
        };

        _tournamentRepo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament(TournamentFormat.DoubleElimination)));
        _matchRepo.GetByCompetitionIdAsync("tournament1", "tenant1")
            .Returns(Result<List<Match>, RepositoryError>.Success(matches));
        _matchRepo.AddRangeAsync(Arg.Any<List<Match>>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("tournament1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }
}
