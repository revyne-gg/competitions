using Xunit;
using competitions.Application;
using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using competitions.Shared;
using NSubstitute;

namespace competitions.Tests.UseCases;

public class DeleteLeagueUseCaseTests
{
    private readonly ILeagueRepository _repo = Substitute.For<ILeagueRepository>();
    private readonly IPermissionService _permissions = Substitute.For<IPermissionService>();

    private DeleteLeagueUseCase CreateSut() => new(_repo, _permissions);

    private static League DefaultLeague() => new()
    {
        Id = "league1", Name = "Test League", TenantId = "tenant1",
    };

    private void SetupOwner() =>
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Owner));

    [Fact]
    public async Task Execute_AnonymisesAndReturnsUnit_WhenOwner()
    {
        SetupOwner();
        var league = DefaultLeague();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(league));
        _repo.Update(Arg.Any<League>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("league1", "org1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.True(league.IsDeleted);
        Assert.Equal("user1", league.DeletedBy);
        Assert.NotNull(league.DeletedAt);
        Assert.StartsWith("Deleted-", league.Name);
        await _repo.Received(1).Update(league);
    }

    [Fact]
    public async Task Execute_ManagerCanDelete()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Manager));
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(DefaultLeague()));
        _repo.Update(Arg.Any<League>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("league1", "org1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Execute_ReturnsForbidden_WhenOrganiserRoleIsMember()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Member));

        var result = await CreateSut().Execute("league1", "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.Forbidden, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsForbidden_WhenOrganiserRoleIsNone()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.None));

        var result = await CreateSut().Execute("league1", "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.Forbidden, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenOrganiserRoleCheckFails()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Failure(PermissionError.InternalError));

        var result = await CreateSut().Execute("league1", "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsNotFound_WhenLeagueDoesNotExist()
    {
        SetupOwner();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Failure(RepositoryError.NotFound));

        var result = await CreateSut().Execute("league1", "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.NotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenLeagueFetchFails()
    {
        SetupOwner();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("league1", "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenUpdateFails()
    {
        SetupOwner();
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(DefaultLeague()));
        _repo.Update(Arg.Any<League>())
            .Returns(Result<Unit, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("league1", "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }
}
