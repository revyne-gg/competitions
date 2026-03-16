using Xunit;
using competitions.Application;
using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Shared;
using NSubstitute;

namespace competitions.Tests.UseCases;

public class DeleteTournamentUseCaseTests
{
    private readonly ITournamentRepository _repo = Substitute.For<ITournamentRepository>();
    private readonly IPermissionService _permissions = Substitute.For<IPermissionService>();

    private DeleteTournamentUseCase CreateSut() => new(_repo, _permissions);

    private static Tournament DefaultTournament() => new()
    {
        Id = "tournament1", Name = "Test Tournament", TenantId = "tenant1",
        Format = TournamentFormat.SingleElimination,
    };

    private void SetupOwner() =>
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Owner));

    [Fact]
    public async Task Execute_AnonymisesAndReturnsUnit_WhenOwner()
    {
        SetupOwner();
        var tournament = DefaultTournament();
        _repo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(tournament));
        _repo.Update(Arg.Any<Tournament>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("tournament1", "org1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.True(tournament.IsDeleted);
        Assert.Equal("user1", tournament.DeletedBy);
        Assert.NotNull(tournament.DeletedAt);
        Assert.StartsWith("Deleted-", tournament.Name);
        await _repo.Received(1).Update(tournament);
    }

    [Fact]
    public async Task Execute_ManagerCanDelete()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Manager));
        _repo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(DefaultTournament()));
        _repo.Update(Arg.Any<Tournament>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("tournament1", "org1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Execute_ReturnsForbidden_WhenOrganiserRoleIsMember()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Member));

        var result = await CreateSut().Execute("tournament1", "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.Forbidden, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsForbidden_WhenOrganiserRoleIsNone()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.None));

        var result = await CreateSut().Execute("tournament1", "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.Forbidden, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenOrganiserRoleCheckFails()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Failure(PermissionError.InternalError));

        var result = await CreateSut().Execute("tournament1", "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsNotFound_WhenTournamentDoesNotExist()
    {
        SetupOwner();
        _repo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Failure(RepositoryError.NotFound));

        var result = await CreateSut().Execute("tournament1", "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.NotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenTournamentFetchFails()
    {
        SetupOwner();
        _repo.GetByIdAsync("tournament1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("tournament1", "org1", "user1", "tenant1");

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

        var result = await CreateSut().Execute("tournament1", "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }
}
