using Xunit;
using competitions.Application;
using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using competitions.Shared;
using NSubstitute;

namespace competitions.Tests.UseCases;

public class UnregisterTeamFromLeagueUseCaseTests
{
    private readonly ILeagueRepository _repo = Substitute.For<ILeagueRepository>();
    private readonly IPermissionService _permissions = Substitute.For<IPermissionService>();

    private UnregisterTeamFromLeagueUseCase CreateSut() => new(_repo, _permissions);

    private static League NotStartedLeague(string id = "league1", string tenant = "tenant1") => new()
    {
        Id = id,
        Name = "League",
        TenantId = tenant,
        State = LeagueStatus.Hidden,
    };

    private void SetupCaptain() =>
        _permissions.GetRoleForUserInTeam("user1", "team1")
            .Returns(Result<TeamMemberRole, PermissionError>.Success(TeamMemberRole.Captain));

    [Fact]
    public async Task Execute_ReturnsUnit_WhenCaptainAndLeagueNotStarted()
    {
        SetupCaptain();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(NotStartedLeague()));
        _repo.UnregisterTeamAsync("league1", "team1", "tenant1")
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));
        _permissions.RemoveTeamFromLeague("team1", "league1")
            .Returns(Result<Unit, PermissionError>.Success(Unit.Value));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        await _repo.Received(1).UnregisterTeamAsync("league1", "team1", "tenant1");
        await _permissions.Received(1).RemoveTeamFromLeague("team1", "league1");
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenTeamRoleCheckFails()
    {
        _permissions.GetRoleForUserInTeam("user1", "team1")
            .Returns(Result<TeamMemberRole, PermissionError>.Failure(PermissionError.InternalError));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsForbidden_WhenTeamRoleIsNone()
    {
        _permissions.GetRoleForUserInTeam("user1", "team1")
            .Returns(Result<TeamMemberRole, PermissionError>.Success(TeamMemberRole.None));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.Forbidden, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsNotFound_WhenLeagueDoesNotExist()
    {
        SetupCaptain();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Failure(RepositoryError.NotFound));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.NotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenLeagueFetchFails()
    {
        SetupCaptain();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenLeagueHasStarted()
    {
        SetupCaptain();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(new League
            {
                Id = "league1", Name = "League", TenantId = "tenant1", State = LeagueStatus.Live,
            }));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsNotFound_WhenUnregisterTeamReturnsNotFound()
    {
        SetupCaptain();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(NotStartedLeague()));
        _repo.UnregisterTeamAsync("league1", "team1", "tenant1")
            .Returns(Result<Unit, RepositoryError>.Failure(RepositoryError.NotFound));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.NotFound, result.Error);
        await _permissions.DidNotReceive().RemoveTeamFromLeague(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Execute_AlsoSucceeds_WhenTeamRoleIsMember()
    {
        _permissions.GetRoleForUserInTeam("user1", "team1")
            .Returns(Result<TeamMemberRole, PermissionError>.Success(TeamMemberRole.Member));
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(NotStartedLeague()));
        _repo.UnregisterTeamAsync("league1", "team1", "tenant1")
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));
        _permissions.RemoveTeamFromLeague("team1", "league1")
            .Returns(Result<Unit, PermissionError>.Success(Unit.Value));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
    }
}
