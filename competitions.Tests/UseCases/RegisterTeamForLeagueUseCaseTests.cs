using Xunit;
using competitions.Application;
using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using competitions.Shared;
using NSubstitute;

namespace competitions.Tests.UseCases;

public class RegisterTeamForLeagueUseCaseTests
{
    private readonly ILeagueRepository _repo = Substitute.For<ILeagueRepository>();
    private readonly IPermissionService _permissions = Substitute.For<IPermissionService>();

    private RegisterTeamForLeagueUseCase CreateSut() => new(_repo, _permissions);

    private static League OpenLeague(string id = "league1", string tenant = "tenant1") => new()
    {
        Id = id,
        Name = "League",
        TenantId = tenant,
        State = LeagueStatus.Public,
        IsRegistrationOpen = true,
    };

    private void SetupCaptain() =>
        _permissions.GetRoleForUserInTeam("user1", "team1")
            .Returns(Result<TeamMemberRole, PermissionError>.Success(TeamMemberRole.Captain));

    [Fact]
    public async Task Execute_ReturnsCompetitionTeam_WhenCaptainAndLeagueOpen()
    {
        SetupCaptain();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(OpenLeague()));
        _repo.GetCompetitionTeamAsync("league1", "team1", "tenant1")
            .Returns(Result<CompetitionTeam?, RepositoryError>.Success(null));
        _repo.RegisterTeamAsync(Arg.Any<CompetitionTeam>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Equal("league1", result.Value!.LeagueId);
        Assert.Equal("team1", result.Value.TeamId);
        Assert.Equal(RegistrationStatus.Pending, result.Value.Status);
    }

    [Fact]
    public async Task Execute_ReturnsExistingRegistration_WhenAlreadyRegistered()
    {
        SetupCaptain();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(OpenLeague()));
        var existing = new CompetitionTeam
        {
            LeagueId = "league1", TeamId = "team1", TenantId = "tenant1",
            CreatedAt = DateTime.UtcNow, Status = RegistrationStatus.Pending,
        };
        _repo.GetCompetitionTeamAsync("league1", "team1", "tenant1")
            .Returns(Result<CompetitionTeam?, RepositoryError>.Success(existing));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Same(existing, result.Value);
        await _repo.DidNotReceive().RegisterTeamAsync(Arg.Any<CompetitionTeam>());
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
    public async Task Execute_ReturnsBadRequest_WhenRegistrationIsClosed()
    {
        SetupCaptain();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(new League
            {
                Id = "league1", Name = "League", TenantId = "tenant1",
                State = LeagueStatus.Public, IsRegistrationOpen = false,
            }));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenGetCompetitionTeamFails()
    {
        SetupCaptain();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(OpenLeague()));
        _repo.GetCompetitionTeamAsync("league1", "team1", "tenant1")
            .Returns(Result<CompetitionTeam?, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenRegisterTeamFails()
    {
        SetupCaptain();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(OpenLeague()));
        _repo.GetCompetitionTeamAsync("league1", "team1", "tenant1")
            .Returns(Result<CompetitionTeam?, RepositoryError>.Success(null));
        _repo.RegisterTeamAsync(Arg.Any<CompetitionTeam>())
            .Returns(Result<Unit, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_AlsoSucceeds_WhenTeamRoleIsMember()
    {
        _permissions.GetRoleForUserInTeam("user1", "team1")
            .Returns(Result<TeamMemberRole, PermissionError>.Success(TeamMemberRole.Member));
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(OpenLeague()));
        _repo.GetCompetitionTeamAsync("league1", "team1", "tenant1")
            .Returns(Result<CompetitionTeam?, RepositoryError>.Success(null));
        _repo.RegisterTeamAsync(Arg.Any<CompetitionTeam>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
    }
}
