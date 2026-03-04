using Xunit;
using competitions.Application;
using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using competitions.Shared;
using NSubstitute;

namespace competitions.Tests.UseCases;

public class CreateDivisionGroupUseCaseTests
{
    private readonly ILeagueRepository _repo = Substitute.For<ILeagueRepository>();
    private readonly IPermissionService _permissions = Substitute.For<IPermissionService>();
    private readonly IIDGenerator _idGenerator = Substitute.For<IIDGenerator>();

    private CreateDivisionGroupUseCase CreateSut() => new(_repo, _permissions, _idGenerator);

    private static Division ExistingDivision(string id = "div1", string leagueId = "league1", string tenant = "tenant1") => new()
    {
        Id = id,
        LeagueId = leagueId,
        Name = "Division",
        Slug = "division",
        TenantId = tenant,
        CreatedAt = DateTime.UtcNow,
    };

    private static League UpcomingLeague(string id = "league1", string tenant = "tenant1") => new()
    {
        Id = id,
        Name = "League",
        TenantId = tenant,
        State = LeagueStatus.Hidden,
    };

    private void SetupOwner() =>
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Owner));

    [Fact]
    public async Task Execute_ReturnsDivisionGroup_WhenAllChecksPass()
    {
        SetupOwner();
        _repo.GetDivisionByIdAsync("div1", "tenant1")
            .Returns(Result<Division, RepositoryError>.Success(ExistingDivision()));
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(UpcomingLeague()));
        _idGenerator.Generate().Returns("grp-id-abcdef");
        _repo.AddDivisionGroupAsync(Arg.Any<DivisionGroup>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("div1", "Group A", 1, "org1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Equal("grp-id-abcdef", result.Value!.Id);
        Assert.Equal("div1", result.Value.DivisionId);
        Assert.Equal("league1", result.Value.LeagueId);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenOrganiserRoleCheckFails()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Failure(PermissionError.InternalError));

        var result = await CreateSut().Execute("div1", "Group A", 1, "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsForbidden_WhenOrganiserRoleIsNone()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.None));

        var result = await CreateSut().Execute("div1", "Group A", 1, "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.Forbidden, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsNotFound_WhenDivisionDoesNotExist()
    {
        SetupOwner();
        _repo.GetDivisionByIdAsync("div1", "tenant1")
            .Returns(Result<Division, RepositoryError>.Failure(RepositoryError.NotFound));

        var result = await CreateSut().Execute("div1", "Group A", 1, "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.NotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenDivisionFetchFails()
    {
        SetupOwner();
        _repo.GetDivisionByIdAsync("div1", "tenant1")
            .Returns(Result<Division, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("div1", "Group A", 1, "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsNotFound_WhenLeagueDoesNotExist()
    {
        SetupOwner();
        _repo.GetDivisionByIdAsync("div1", "tenant1")
            .Returns(Result<Division, RepositoryError>.Success(ExistingDivision()));
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Failure(RepositoryError.NotFound));

        var result = await CreateSut().Execute("div1", "Group A", 1, "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.NotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenLeagueHasStarted()
    {
        SetupOwner();
        _repo.GetDivisionByIdAsync("div1", "tenant1")
            .Returns(Result<Division, RepositoryError>.Success(ExistingDivision()));
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(new League
            {
                Id = "league1", Name = "L", TenantId = "tenant1", State = LeagueStatus.Live,
            }));

        var result = await CreateSut().Execute("div1", "Group A", 1, "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenAddDivisionGroupFails()
    {
        SetupOwner();
        _repo.GetDivisionByIdAsync("div1", "tenant1")
            .Returns(Result<Division, RepositoryError>.Success(ExistingDivision()));
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(UpcomingLeague()));
        _idGenerator.Generate().Returns("grp-id-abcdef");
        _repo.AddDivisionGroupAsync(Arg.Any<DivisionGroup>())
            .Returns(Result<Unit, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("div1", "Group A", 1, "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }
}
