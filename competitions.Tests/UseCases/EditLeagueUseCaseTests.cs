using Xunit;
using competitions.Application;
using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using competitions.Shared;
using NSubstitute;

namespace competitions.Tests.UseCases;

public class EditLeagueUseCaseTests
{
    private readonly ILeagueRepository _repo = Substitute.For<ILeagueRepository>();
    private readonly IPermissionService _permissions = Substitute.For<IPermissionService>();

    private EditLeagueUseCase CreateSut() => new(_repo, _permissions);

    private static League HiddenLeague() => new()
    {
        Id = "league1", Name = "Old Name", Description = "Old Desc", TenantId = "tenant1",
        State = LeagueStatus.Hidden, Game = "CS2", BestOf = 1,
    };

    private static League LiveLeague() => new()
    {
        Id = "league1", Name = "Old Name", Description = "Old Desc", TenantId = "tenant1",
        State = LeagueStatus.Live, Game = "CS2", BestOf = 1,
    };

    private void SetupOwner() =>
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Owner));

    [Fact]
    public async Task Execute_UpdatesAllFields_WhenNotStarted()
    {
        SetupOwner();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(HiddenLeague()));
        _repo.Update(Arg.Any<League>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute(
            "league1", "New Name", "New Desc", "Valorant", 3,
            new List<string> { "Haven" }, LeagueLegs.TwoLegs, LeagueStatus.Public,
            true, true, "org1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        var league = result.Value!;
        Assert.Equal("New Name", league.Name);
        Assert.Equal("New Desc", league.Description);
        Assert.Equal("Valorant", league.Game);
        Assert.Equal(3, league.BestOf);
        Assert.Equal(new List<string> { "Haven" }, league.MapPool);
        Assert.Equal(LeagueLegs.TwoLegs, league.Legs);
        Assert.Equal(LeagueStatus.Public, league.State);
        Assert.True(league.IsRegistrationOpen);
        Assert.True(league.IsAutoAccept);
    }

    [Fact]
    public async Task Execute_UpdatesOnlyBasicFields_WhenStarted()
    {
        SetupOwner();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(LiveLeague()));
        _repo.Update(Arg.Any<League>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute(
            "league1", "New Name", "New Desc", null, null,
            null, null, null, null, null, "org1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Equal("New Name", result.Value!.Name);
        Assert.Equal("New Desc", result.Value.Description);
        Assert.Equal("CS2", result.Value.Game); // unchanged
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenStartedAndConfigFieldsChanged()
    {
        SetupOwner();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(LiveLeague()));

        var result = await CreateSut().Execute(
            "league1", "New Name", null, "Valorant", null,
            null, null, null, null, null, "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenStartedAndBestOfChanged()
    {
        SetupOwner();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(LiveLeague()));

        var result = await CreateSut().Execute(
            "league1", null, null, null, 5,
            null, null, null, null, null, "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenStartedAndStateChanged()
    {
        SetupOwner();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(LiveLeague()));

        var result = await CreateSut().Execute(
            "league1", null, null, null, null,
            null, null, LeagueStatus.Finished, null, null, "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsForbidden_WhenOrganiserRoleIsMember()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Member));

        var result = await CreateSut().Execute(
            "league1", "Name", null, null, null,
            null, null, null, null, null, "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.Forbidden, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenOrganiserRoleCheckFails()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Failure(PermissionError.InternalError));

        var result = await CreateSut().Execute(
            "league1", "Name", null, null, null,
            null, null, null, null, null, "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsNotFound_WhenLeagueDoesNotExist()
    {
        SetupOwner();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Failure(RepositoryError.NotFound));

        var result = await CreateSut().Execute(
            "league1", "Name", null, null, null,
            null, null, null, null, null, "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.NotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenUpdateFails()
    {
        SetupOwner();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(HiddenLeague()));
        _repo.Update(Arg.Any<League>())
            .Returns(Result<Unit, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute(
            "league1", "New Name", null, null, null,
            null, null, null, null, null, "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ManagerCanEdit()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Manager));
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(HiddenLeague()));
        _repo.Update(Arg.Any<League>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute(
            "league1", "Updated", null, null, null,
            null, null, null, null, null, "org1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Execute_SkipsNullFields()
    {
        SetupOwner();
        var league = HiddenLeague();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(league));
        _repo.Update(Arg.Any<League>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute(
            "league1", null, null, null, null,
            null, null, null, null, null, "org1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Equal("Old Name", result.Value!.Name);
        Assert.Equal("Old Desc", result.Value.Description);
    }
}
