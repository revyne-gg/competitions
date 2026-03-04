using Xunit;
using competitions.Application;
using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using competitions.Shared;
using NSubstitute;

namespace competitions.Tests.UseCases;

public class CreateLeagueUseCaseTests
{
    private readonly ILeagueRepository _repo = Substitute.For<ILeagueRepository>();
    private readonly IPermissionService _permissions = Substitute.For<IPermissionService>();
    private readonly IIDGenerator _idGenerator = Substitute.For<IIDGenerator>();
    private readonly IDiscriminatorGenerator _discriminatorGenerator = Substitute.For<IDiscriminatorGenerator>();

    private CreateLeagueUseCase CreateSut() =>
        new(_repo, _permissions, _idGenerator, _discriminatorGenerator);

    private static LeagueConfig DefaultConfig() => new() { Name = "Test League", Description = "Desc" };

    [Fact]
    public async Task Execute_ReturnsLeague_WhenOwnerAndRealmAdmin()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Owner));
        _permissions.GetRoleForUserInRealm("user1", "realm1")
            .Returns(Result<RealmMemberRole, PermissionError>.Success(RealmMemberRole.Admin));
        _discriminatorGenerator.Generate().Returns("ABC1");
        _repo.GetByNameAndDiscriminatorAsync("Test League", "ABC1", "tenant1")
            .Returns(Result<League, RepositoryError>.Failure(RepositoryError.NotFound));
        _idGenerator.Generate().Returns("league-id-1");
        _repo.AddAsync(Arg.Any<League>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute(DefaultConfig(), "org1", "realm1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Equal("league-id-1", result.Value!.Id);
        Assert.Equal("Test League", result.Value.Name);
        Assert.Equal("ABC1", result.Value.Discriminator);
    }

    [Fact]
    public async Task Execute_ReturnsLeague_WhenManagerAndRealmAdmin()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Manager));
        _permissions.GetRoleForUserInRealm("user1", "realm1")
            .Returns(Result<RealmMemberRole, PermissionError>.Success(RealmMemberRole.Admin));
        _discriminatorGenerator.Generate().Returns("XYZ9");
        _repo.GetByNameAndDiscriminatorAsync("Test League", "XYZ9", "tenant1")
            .Returns(Result<League, RepositoryError>.Failure(RepositoryError.NotFound));
        _idGenerator.Generate().Returns("league-id-2");
        _repo.AddAsync(Arg.Any<League>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute(DefaultConfig(), "org1", "realm1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenOrganiserRoleCheckFails()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Failure(PermissionError.InternalError));

        var result = await CreateSut().Execute(DefaultConfig(), "org1", "realm1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsForbidden_WhenOrganiserRoleIsMember()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Member));

        var result = await CreateSut().Execute(DefaultConfig(), "org1", "realm1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.Forbidden, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsForbidden_WhenOrganiserRoleIsNone()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.None));

        var result = await CreateSut().Execute(DefaultConfig(), "org1", "realm1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.Forbidden, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenRealmRoleCheckFails()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Owner));
        _permissions.GetRoleForUserInRealm("user1", "realm1")
            .Returns(Result<RealmMemberRole, PermissionError>.Failure(PermissionError.InternalError));

        var result = await CreateSut().Execute(DefaultConfig(), "org1", "realm1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsForbidden_WhenRealmRoleIsNotAdmin()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Owner));
        _permissions.GetRoleForUserInRealm("user1", "realm1")
            .Returns(Result<RealmMemberRole, PermissionError>.Success(RealmMemberRole.Moderator));

        var result = await CreateSut().Execute(DefaultConfig(), "org1", "realm1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.Forbidden, result.Error);
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenDiscriminatorCheckFails()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Owner));
        _permissions.GetRoleForUserInRealm("user1", "realm1")
            .Returns(Result<RealmMemberRole, PermissionError>.Success(RealmMemberRole.Admin));
        _discriminatorGenerator.Generate().Returns("ABC1");
        _repo.GetByNameAndDiscriminatorAsync("Test League", "ABC1", "tenant1")
            .Returns(Result<League, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute(DefaultConfig(), "org1", "realm1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_RetriesDiscriminator_WhenCollisionDetected()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Owner));
        _permissions.GetRoleForUserInRealm("user1", "realm1")
            .Returns(Result<RealmMemberRole, PermissionError>.Success(RealmMemberRole.Admin));

        var existing = new League { Id = "x", Name = "Test League", Discriminator = "DUPE", TenantId = "tenant1" };
        _discriminatorGenerator.Generate().Returns("DUPE", "UNIQ");
        _repo.GetByNameAndDiscriminatorAsync("Test League", "DUPE", "tenant1")
            .Returns(Result<League, RepositoryError>.Success(existing));
        _repo.GetByNameAndDiscriminatorAsync("Test League", "UNIQ", "tenant1")
            .Returns(Result<League, RepositoryError>.Failure(RepositoryError.NotFound));
        _idGenerator.Generate().Returns("new-id");
        _repo.AddAsync(Arg.Any<League>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute(DefaultConfig(), "org1", "realm1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Equal("UNIQ", result.Value!.Discriminator);
        await _discriminatorGenerator.Received(2).Generate();
    }
}
