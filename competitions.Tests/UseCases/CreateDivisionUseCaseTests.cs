using Xunit;
using competitions.Application;
using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using competitions.Shared;
using NSubstitute;

namespace competitions.Tests.UseCases;

public class CreateDivisionUseCaseTests
{
    private readonly ILeagueRepository _repo = Substitute.For<ILeagueRepository>();
    private readonly IPermissionService _permissions = Substitute.For<IPermissionService>();
    private readonly IIDGenerator _idGenerator = Substitute.For<IIDGenerator>();

    private CreateDivisionUseCase CreateSut() => new(_repo, _permissions, _idGenerator);

    private static League UpcomingLeague(string id = "league1", string tenant = "tenant1") => new()
    {
        Id = id,
        Name = "League",
        TenantId = tenant,
        State = LeagueStatus.Hidden,
    };

    [Fact]
    public async Task Execute_ReturnsDivision_WhenOwnerAndLeagueExists()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Owner));
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(UpcomingLeague()));
        _idGenerator.Generate().Returns("div-id-1");
        _repo.AddDivisionAsync(Arg.Any<Division>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute("league1", "Div A", 1, 3, 4, "tag", "org1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Equal("div-id-1", result.Value!.Id);
        Assert.Equal("Div A", result.Value.Name);
        Assert.Equal("league1", result.Value.LeagueId);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenOrganiserRoleCheckFails()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Failure(PermissionError.InternalError));

        var result = await CreateSut().Execute("league1", "Div A", 1, 3, 4, "tag", "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsForbidden_WhenOrganiserRoleIsMember()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Member));

        var result = await CreateSut().Execute("league1", "Div A", 1, 3, 4, "tag", "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.Forbidden, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsNotFound_WhenLeagueDoesNotExist()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Owner));
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Failure(RepositoryError.NotFound));

        var result = await CreateSut().Execute("league1", "Div A", 1, 3, 4, "tag", "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.NotFound, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenLeagueFetchFails()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Owner));
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("league1", "Div A", 1, 3, 4, "tag", "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsBadRequest_WhenLeagueHasStarted()
    {
        var startedLeague = new League
        {
            Id = "league1",
            Name = "League",
            TenantId = "tenant1",
            State = LeagueStatus.Live,
        };
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Owner));
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(startedLeague));

        var result = await CreateSut().Execute("league1", "Div A", 1, 3, 4, "tag", "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.BadRequest, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenAddDivisionFails()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Owner));
        _repo.GetByIdAsync("league1", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(UpcomingLeague()));
        _idGenerator.Generate().Returns("div-id-1");
        _repo.AddDivisionAsync(Arg.Any<Division>())
            .Returns(Result<Unit, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute("league1", "Div A", 1, 3, 4, "tag", "org1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }
}
