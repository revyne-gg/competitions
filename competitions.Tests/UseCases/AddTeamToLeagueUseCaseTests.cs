using Xunit;
using competitions.Application;
using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using competitions.Shared;
using NSubstitute;

namespace competitions.Tests.UseCases;

public class AddTeamToLeagueUseCaseTests
{
    private readonly ILeagueRepository _repo = Substitute.For<ILeagueRepository>();
    private readonly IPermissionService _permissions = Substitute.For<IPermissionService>();

    private AddTeamToLeagueUseCase CreateSut() => new(_repo, _permissions);

    private static DivisionGroup ExistingGroup(string id = "grp1", string leagueId = "league1", string tenant = "tenant1") => new()
    {
        Id = id,
        DivisionId = "div1",
        LeagueId = leagueId,
        Name = "Group A",
        Slug = "group-a",
        TenantId = tenant,
        CreatedAt = DateTime.UtcNow,
    };

    private void SetupOwner() =>
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Owner));

    [Fact]
    public async Task Execute_CreatesCompetitionTeamAndAddsToGroup_WhenNoExistingRegistration()
    {
        SetupOwner();
        var group = ExistingGroup();
        var updatedGroup = ExistingGroup();
        _repo.GetDivisionGroupByIdAsync("grp1", "tenant1")
            .Returns(Result<DivisionGroup, RepositoryError>.Success(group));
        _repo.GetCompetitionTeamAsync("league1", "team1", "tenant1")
            .Returns(Result<CompetitionTeam?, RepositoryError>.Success(null));
        _repo.RegisterTeamAsync(Arg.Any<CompetitionTeam>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));
        _permissions.AddTeamToLeague("team1", "league1")
            .Returns(Result<Unit, PermissionError>.Success(Unit.Value));
        _repo.AddTeamToDivisionGroupAsync("grp1", "team1")
            .Returns(Result<DivisionGroup, RepositoryError>.Success(updatedGroup));

        var result = await CreateSut().Execute("grp1", "team1", "user1", "org1", "tenant1");

        Assert.True(result.IsSuccess);
        await _repo.Received(1).RegisterTeamAsync(Arg.Is<CompetitionTeam>(t =>
            t.LeagueId == "league1" &&
            t.TeamId == "team1" &&
            t.Status == RegistrationStatus.Approved));
        await _permissions.Received(1).AddTeamToLeague("team1", "league1");
        await _repo.Received(1).AddTeamToDivisionGroupAsync("grp1", "team1");
    }

    [Fact]
    public async Task Execute_SkipsRegistration_WhenCompetitionTeamAlreadyExists()
    {
        SetupOwner();
        var group = ExistingGroup();
        var existingTeam = new CompetitionTeam
        {
            LeagueId = "league1", TeamId = "team1", TenantId = "tenant1",
            CreatedAt = DateTime.UtcNow, Status = RegistrationStatus.Approved,
        };
        var updatedGroup = ExistingGroup();
        _repo.GetDivisionGroupByIdAsync("grp1", "tenant1")
            .Returns(Result<DivisionGroup, RepositoryError>.Success(group));
        _repo.GetCompetitionTeamAsync("league1", "team1", "tenant1")
            .Returns(Result<CompetitionTeam?, RepositoryError>.Success(existingTeam));
        _repo.AddTeamToDivisionGroupAsync("grp1", "team1")
            .Returns(Result<DivisionGroup, RepositoryError>.Success(updatedGroup));

        var result = await CreateSut().Execute("grp1", "team1", "user1", "org1", "tenant1");

        Assert.True(result.IsSuccess);
        await _repo.DidNotReceive().RegisterTeamAsync(Arg.Any<CompetitionTeam>());
        await _permissions.DidNotReceive().AddTeamToLeague(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenOrganiserRoleCheckFails()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Failure(PermissionError.InternalError));

        var result = await CreateSut().Execute("grp1", "team1", "user1", "org1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsForbidden_WhenOrganiserRoleIsMember()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Member));

        var result = await CreateSut().Execute("grp1", "team1", "user1", "org1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.Forbidden, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsNotFound_WhenGroupDoesNotExist()
    {
        SetupOwner();
        _repo.GetDivisionGroupByIdAsync("grp1", "tenant1")
            .Returns(Result<DivisionGroup, RepositoryError>.Failure(RepositoryError.NotFound));

        var result = await CreateSut().Execute("grp1", "team1", "user1", "org1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.NotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenGetCompetitionTeamFails()
    {
        SetupOwner();
        _repo.GetDivisionGroupByIdAsync("grp1", "tenant1")
            .Returns(Result<DivisionGroup, RepositoryError>.Success(ExistingGroup()));
        _repo.GetCompetitionTeamAsync("league1", "team1", "tenant1")
            .Returns(Result<CompetitionTeam?, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("grp1", "team1", "user1", "org1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenRegisterTeamFails()
    {
        SetupOwner();
        _repo.GetDivisionGroupByIdAsync("grp1", "tenant1")
            .Returns(Result<DivisionGroup, RepositoryError>.Success(ExistingGroup()));
        _repo.GetCompetitionTeamAsync("league1", "team1", "tenant1")
            .Returns(Result<CompetitionTeam?, RepositoryError>.Success(null));
        _repo.RegisterTeamAsync(Arg.Any<CompetitionTeam>())
            .Returns(Result<Unit, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("grp1", "team1", "user1", "org1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsNotFound_WhenAddTeamToDivisionGroupReturnsNotFound()
    {
        SetupOwner();
        _repo.GetDivisionGroupByIdAsync("grp1", "tenant1")
            .Returns(Result<DivisionGroup, RepositoryError>.Success(ExistingGroup()));
        _repo.GetCompetitionTeamAsync("league1", "team1", "tenant1")
            .Returns(Result<CompetitionTeam?, RepositoryError>.Success(null));
        _repo.RegisterTeamAsync(Arg.Any<CompetitionTeam>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));
        _permissions.AddTeamToLeague("team1", "league1")
            .Returns(Result<Unit, PermissionError>.Success(Unit.Value));
        _repo.AddTeamToDivisionGroupAsync("grp1", "team1")
            .Returns(Result<DivisionGroup, RepositoryError>.Failure(RepositoryError.NotFound));

        var result = await CreateSut().Execute("grp1", "team1", "user1", "org1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.NotFound, result.Error);
    }
}
