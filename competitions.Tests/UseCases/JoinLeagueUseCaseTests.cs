using Xunit;
using competitions.Application;
using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using competitions.Shared;
using NSubstitute;

namespace competitions.Tests.UseCases;

public class JoinLeagueUseCaseTests
{
    private readonly ILeagueRepository _repo = Substitute.For<ILeagueRepository>();
    private readonly IPermissionService _permissions = Substitute.For<IPermissionService>();

    private JoinLeagueUseCase CreateSut() => new(_repo, _permissions);

    private static League OpenLeague(bool autoAccept = false) => new()
    {
        Id = "league1", Name = "League", TenantId = "tenant1",
        IsRegistrationOpen = true, IsAutoAccept = autoAccept,
    };

    private static League ClosedLeague() => new()
    {
        Id = "league1", Name = "League", TenantId = "tenant1",
        IsRegistrationOpen = false,
    };

    private void SetupCaptain() =>
        _permissions.GetRoleForUserInTeam("user1", "team1")
            .Returns(Result<TeamMemberRole, PermissionError>.Success(TeamMemberRole.Captain));

    [Fact]
    public async Task Execute_ReturnsPendingRegistration_WhenNotAutoAccept()
    {
        SetupCaptain();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(OpenLeague(autoAccept: false)));
        _repo.GetCompetitionTeamAsync("league1", "team1", "tenant1")
            .Returns(Result<CompetitionTeam?, RepositoryError>.Success((CompetitionTeam?)null));
        _repo.RegisterTeamAsync(Arg.Any<CompetitionTeam>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Equal(RegistrationStatus.Pending, result.Value!.Status);
        Assert.Equal("league1", result.Value.LeagueId);
        Assert.Equal("team1", result.Value.TeamId);
        await _permissions.DidNotReceive().AddTeamToLeague(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Execute_ReturnsApprovedRegistration_WhenAutoAccept()
    {
        SetupCaptain();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(OpenLeague(autoAccept: true)));
        _repo.GetCompetitionTeamAsync("league1", "team1", "tenant1")
            .Returns(Result<CompetitionTeam?, RepositoryError>.Success((CompetitionTeam?)null));
        _repo.RegisterTeamAsync(Arg.Any<CompetitionTeam>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));
        _permissions.AddTeamToLeague("team1", "league1")
            .Returns(Result<Unit, PermissionError>.Success(Unit.Value));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Equal(RegistrationStatus.Approved, result.Value!.Status);
        await _permissions.Received(1).AddTeamToLeague("team1", "league1");
    }

    [Fact]
    public async Task Execute_ReturnsExistingRegistration_WhenAlreadyRegistered()
    {
        SetupCaptain();
        var existing = new CompetitionTeam
        {
            LeagueId = "league1", TeamId = "team1", TenantId = "tenant1",
            CreatedAt = DateTime.UtcNow, Status = RegistrationStatus.Pending,
        };
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(OpenLeague()));
        _repo.GetCompetitionTeamAsync("league1", "team1", "tenant1")
            .Returns(Result<CompetitionTeam?, RepositoryError>.Success(existing));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Same(existing, result.Value);
        await _repo.DidNotReceive().RegisterTeamAsync(Arg.Any<CompetitionTeam>());
    }

    [Fact]
    public async Task Execute_MemberCanJoin()
    {
        _permissions.GetRoleForUserInTeam("user1", "team1")
            .Returns(Result<TeamMemberRole, PermissionError>.Success(TeamMemberRole.Member));
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(OpenLeague()));
        _repo.GetCompetitionTeamAsync("league1", "team1", "tenant1")
            .Returns(Result<CompetitionTeam?, RepositoryError>.Success((CompetitionTeam?)null));
        _repo.RegisterTeamAsync(Arg.Any<CompetitionTeam>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
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
    public async Task Execute_ReturnsInternalError_WhenTeamRoleCheckFails()
    {
        _permissions.GetRoleForUserInTeam("user1", "team1")
            .Returns(Result<TeamMemberRole, PermissionError>.Failure(PermissionError.InternalError));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
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
    public async Task Execute_ReturnsBadRequest_WhenRegistrationClosed()
    {
        SetupCaptain();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(ClosedLeague()));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenExistingCheckFails()
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
    public async Task Execute_ReturnsInternalError_WhenRegisterFails()
    {
        SetupCaptain();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(OpenLeague()));
        _repo.GetCompetitionTeamAsync("league1", "team1", "tenant1")
            .Returns(Result<CompetitionTeam?, RepositoryError>.Success((CompetitionTeam?)null));
        _repo.RegisterTeamAsync(Arg.Any<CompetitionTeam>())
            .Returns(Result<Unit, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenAutoAcceptPermissionFails()
    {
        SetupCaptain();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(OpenLeague(autoAccept: true)));
        _repo.GetCompetitionTeamAsync("league1", "team1", "tenant1")
            .Returns(Result<CompetitionTeam?, RepositoryError>.Success((CompetitionTeam?)null));
        _repo.RegisterTeamAsync(Arg.Any<CompetitionTeam>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));
        _permissions.AddTeamToLeague("team1", "league1")
            .Returns(Result<Unit, PermissionError>.Failure(PermissionError.InternalError));

        var result = await CreateSut().Execute("league1", "team1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }
}
