using Xunit;
using competitions.Application;
using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Competitions.Matches.Models;
using competitions.Domain.Models;
using competitions.Shared;
using NSubstitute;

namespace competitions.Tests.UseCases;

public class CreateMatchesForLeagueUseCaseTests
{
    private readonly ILeagueRepository _leagueRepo = Substitute.For<ILeagueRepository>();
    private readonly IMatchRepository _matchRepo = Substitute.For<IMatchRepository>();
    private readonly IIDGenerator _idGenerator = Substitute.For<IIDGenerator>();

    private CreateMatchesForLeagueUseCase CreateSut() => new(_leagueRepo, _matchRepo, _idGenerator);

    private static League DefaultLeague(LeagueLegs legs = LeagueLegs.OneLeg) => new()
    {
        Id = "league1",
        Name = "League",
        TenantId = "tenant1",
        Legs = legs,
    };

    private static Division DefaultDivision(string id = "div1") => new()
    {
        Id = id,
        LeagueId = "league1",
        Name = "Division",
        TenantId = "tenant1",
    };

    private static DivisionGroup GroupWithTeams(string id, params string[] teamIds) => new()
    {
        Id = id,
        DivisionId = "div1",
        LeagueId = "league1",
        Name = "Group",
        TenantId = "tenant1",
        Teams = teamIds.Select(t => new DivisionGroupTeam { TeamId = t, GroupId = id, LeagueId = "league1" }).ToList(),
    };

    private int _idCounter;
    private void SetupIdGenerator()
    {
        _idCounter = 0;
        _idGenerator.Generate().Returns(_ => Task.FromResult($"id{++_idCounter}"));
    }

    [Fact]
    public async Task Execute_ReturnsMatches_ForSingleGroupWithThreeTeams()
    {
        SetupIdGenerator();
        _leagueRepo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(DefaultLeague()));
        _leagueRepo.GetDivisionsByLeagueAsync("league1", "tenant1")
            .Returns(Result<List<Division>, RepositoryError>.Success(new List<Division> { DefaultDivision() }));
        _leagueRepo.GetDivisionGroupsByDivisionAsync("div1", "tenant1")
            .Returns(Result<List<DivisionGroup>, RepositoryError>.Success(new List<DivisionGroup>
            {
                GroupWithTeams("g1", "teamA", "teamB", "teamC"),
            }));
        _matchRepo.AddRangeAsync(Arg.Any<List<Match>>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("league1", "tenant1");

        Assert.True(result.IsSuccess);
        // 3 teams → 3 round-robin pairs: AB, AC, BC
        Assert.Equal(3, result.Value!.Count);
        Assert.All(result.Value, m => Assert.Equal("league1", m.CompetitionId));
        Assert.All(result.Value, m => Assert.StartsWith("match_", m.Id));
        Assert.All(result.Value, m => Assert.Equal("tenant1", m.TenantId));
    }

    [Fact]
    public async Task Execute_DoublesPairs_WhenTwoLegs()
    {
        SetupIdGenerator();
        _leagueRepo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(DefaultLeague(LeagueLegs.TwoLegs)));
        _leagueRepo.GetDivisionsByLeagueAsync("league1", "tenant1")
            .Returns(Result<List<Division>, RepositoryError>.Success(new List<Division> { DefaultDivision() }));
        _leagueRepo.GetDivisionGroupsByDivisionAsync("div1", "tenant1")
            .Returns(Result<List<DivisionGroup>, RepositoryError>.Success(new List<DivisionGroup>
            {
                GroupWithTeams("g1", "teamA", "teamB"),
            }));
        _matchRepo.AddRangeAsync(Arg.Any<List<Match>>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("league1", "tenant1");

        Assert.True(result.IsSuccess);
        // 2 teams → 1 pair, TwoLegs → 2 matches (home/away reversed)
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal("teamA", result.Value[0].HomeTeamId);
        Assert.Equal("teamB", result.Value[0].AwayTeamId);
        Assert.Equal("teamB", result.Value[1].HomeTeamId);
        Assert.Equal("teamA", result.Value[1].AwayTeamId);
    }

    [Fact]
    public async Task Execute_ReturnsNotFound_WhenLeagueDoesNotExist()
    {
        _leagueRepo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Failure(RepositoryError.NotFound));

        var result = await CreateSut().Execute("league1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.NotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenLeagueFetchFails()
    {
        _leagueRepo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("league1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenDivisionsFetchFails()
    {
        _leagueRepo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(DefaultLeague()));
        _leagueRepo.GetDivisionsByLeagueAsync("league1", "tenant1")
            .Returns(Result<List<Division>, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("league1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenGroupsFetchFails()
    {
        _leagueRepo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(DefaultLeague()));
        _leagueRepo.GetDivisionsByLeagueAsync("league1", "tenant1")
            .Returns(Result<List<Division>, RepositoryError>.Success(new List<Division> { DefaultDivision() }));
        _leagueRepo.GetDivisionGroupsByDivisionAsync("div1", "tenant1")
            .Returns(Result<List<DivisionGroup>, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("league1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenNoMatchesGenerated()
    {
        _leagueRepo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(DefaultLeague()));
        _leagueRepo.GetDivisionsByLeagueAsync("league1", "tenant1")
            .Returns(Result<List<Division>, RepositoryError>.Success(new List<Division> { DefaultDivision() }));
        _leagueRepo.GetDivisionGroupsByDivisionAsync("div1", "tenant1")
            .Returns(Result<List<DivisionGroup>, RepositoryError>.Success(new List<DivisionGroup>
            {
                GroupWithTeams("g1", "teamA"), // only 1 team → no pairs
            }));

        var result = await CreateSut().Execute("league1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenNoDivisions()
    {
        _leagueRepo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(DefaultLeague()));
        _leagueRepo.GetDivisionsByLeagueAsync("league1", "tenant1")
            .Returns(Result<List<Division>, RepositoryError>.Success(new List<Division>()));

        var result = await CreateSut().Execute("league1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenSaveFails()
    {
        SetupIdGenerator();
        _leagueRepo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(DefaultLeague()));
        _leagueRepo.GetDivisionsByLeagueAsync("league1", "tenant1")
            .Returns(Result<List<Division>, RepositoryError>.Success(new List<Division> { DefaultDivision() }));
        _leagueRepo.GetDivisionGroupsByDivisionAsync("div1", "tenant1")
            .Returns(Result<List<DivisionGroup>, RepositoryError>.Success(new List<DivisionGroup>
            {
                GroupWithTeams("g1", "teamA", "teamB"),
            }));
        _matchRepo.AddRangeAsync(Arg.Any<List<Match>>())
            .Returns(Result<Unit, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("league1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_GeneratesMatchesAcrossMultipleDivisionsAndGroups()
    {
        SetupIdGenerator();
        _leagueRepo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(DefaultLeague()));
        _leagueRepo.GetDivisionsByLeagueAsync("league1", "tenant1")
            .Returns(Result<List<Division>, RepositoryError>.Success(new List<Division>
            {
                DefaultDivision("div1"),
                DefaultDivision("div2"),
            }));
        _leagueRepo.GetDivisionGroupsByDivisionAsync("div1", "tenant1")
            .Returns(Result<List<DivisionGroup>, RepositoryError>.Success(new List<DivisionGroup>
            {
                GroupWithTeams("g1", "t1", "t2"), // 1 pair
            }));
        _leagueRepo.GetDivisionGroupsByDivisionAsync("div2", "tenant1")
            .Returns(Result<List<DivisionGroup>, RepositoryError>.Success(new List<DivisionGroup>
            {
                GroupWithTeams("g2", "t3", "t4"), // 1 pair
            }));
        _matchRepo.AddRangeAsync(Arg.Any<List<Match>>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("league1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task Execute_SkipsGroupsWithFewerThanTwoTeams()
    {
        SetupIdGenerator();
        _leagueRepo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(DefaultLeague()));
        _leagueRepo.GetDivisionsByLeagueAsync("league1", "tenant1")
            .Returns(Result<List<Division>, RepositoryError>.Success(new List<Division> { DefaultDivision() }));
        _leagueRepo.GetDivisionGroupsByDivisionAsync("div1", "tenant1")
            .Returns(Result<List<DivisionGroup>, RepositoryError>.Success(new List<DivisionGroup>
            {
                GroupWithTeams("g1", "teamA"), // 1 team → skip
                GroupWithTeams("g2", "teamB", "teamC"), // 2 teams → 1 pair
            }));
        _matchRepo.AddRangeAsync(Arg.Any<List<Match>>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("league1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task Execute_RoundIsNull_ForLeagueMatches()
    {
        SetupIdGenerator();
        _leagueRepo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(DefaultLeague()));
        _leagueRepo.GetDivisionsByLeagueAsync("league1", "tenant1")
            .Returns(Result<List<Division>, RepositoryError>.Success(new List<Division> { DefaultDivision() }));
        _leagueRepo.GetDivisionGroupsByDivisionAsync("div1", "tenant1")
            .Returns(Result<List<DivisionGroup>, RepositoryError>.Success(new List<DivisionGroup>
            {
                GroupWithTeams("g1", "teamA", "teamB"),
            }));
        _matchRepo.AddRangeAsync(Arg.Any<List<Match>>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("league1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.All(result.Value!, m => Assert.Null(m.Round));
    }
}
