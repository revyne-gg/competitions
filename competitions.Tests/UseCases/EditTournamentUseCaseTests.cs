using Xunit;
using competitions.Application;
using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Shared;
using NSubstitute;

namespace competitions.Tests.UseCases;

public class EditTournamentUseCaseTests
{
    private readonly ITournamentRepository _repo = Substitute.For<ITournamentRepository>();
    private readonly IPermissionService _permissions = Substitute.For<IPermissionService>();

    private EditTournamentUseCase CreateSut() => new(_repo, _permissions);

    private static Tournament DefaultTournament() => new()
    {
        Id = "tournament1", Name = "Old Name", Description = "Old Desc", TenantId = "tenant1",
        Format = TournamentFormat.SingleElimination, Game = "CS2", BestOf = 1,
    };

    private void SetupOwner() =>
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Owner));

    [Fact]
    public async Task Execute_UpdatesAllFields()
    {
        SetupOwner();
        _repo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament()));
        _repo.Update(Arg.Any<Tournament>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute(
            "tournament1", "New Name", "New Desc", "Valorant", 3,
            new List<string> { "Haven" }, true, "org1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        var t = result.Value!;
        Assert.Equal("New Name", t.Name);
        Assert.Equal("New Desc", t.Description);
        Assert.Equal("Valorant", t.Game);
        Assert.Equal(3, t.BestOf);
        Assert.Equal(new List<string> { "Haven" }, t.MapPool);
        Assert.True(t.IsAutoAccept);
    }

    [Fact]
    public async Task Execute_SkipsNullFields()
    {
        SetupOwner();
        _repo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament()));
        _repo.Update(Arg.Any<Tournament>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute(
            "tournament1", null, null, null, null,
            null, null, "org1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Equal("Old Name", result.Value!.Name);
        Assert.Equal("CS2", result.Value.Game);
    }

    [Fact]
    public async Task Execute_ReturnsForbidden_WhenOrganiserRoleIsMember()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Member));

        var result = await CreateSut().Execute(
            "tournament1", "Name", null, null, null,
            null, null, "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.Forbidden, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenOrganiserRoleCheckFails()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Failure(PermissionError.InternalError));

        var result = await CreateSut().Execute(
            "tournament1", "Name", null, null, null,
            null, null, "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsNotFound_WhenTournamentDoesNotExist()
    {
        SetupOwner();
        _repo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Failure(RepositoryError.NotFound));

        var result = await CreateSut().Execute(
            "tournament1", "Name", null, null, null,
            null, null, "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.NotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenTournamentFetchFails()
    {
        SetupOwner();
        _repo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute(
            "tournament1", "Name", null, null, null,
            null, null, "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenUpdateFails()
    {
        SetupOwner();
        _repo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament()));
        _repo.Update(Arg.Any<Tournament>())
            .Returns(Result<Unit, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute(
            "tournament1", "Name", null, null, null,
            null, null, "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ManagerCanEdit()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Manager));
        _repo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament()));
        _repo.Update(Arg.Any<Tournament>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute(
            "tournament1", "Updated", null, null, null,
            null, null, "org1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
    }
}
