using Xunit;
using competitions.Application;
using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Domain.Models;
using competitions.Shared;
using NSubstitute;

namespace competitions.Tests.UseCases;

public class CreateTournamentUseCaseTests
{
    private readonly ITournamentRepository _repo = Substitute.For<ITournamentRepository>();
    private readonly IPermissionService _permissions = Substitute.For<IPermissionService>();
    private readonly IIDGenerator _idGenerator = Substitute.For<IIDGenerator>();
    private readonly IDiscriminatorGenerator _discriminatorGenerator = Substitute.For<IDiscriminatorGenerator>();

    private CreateTournamentUseCase CreateSut() =>
        new(_repo, _permissions, _idGenerator, _discriminatorGenerator);

    private static TournamentConfig DefaultConfig() => new()
    {
        Name = "Test Tournament",
        Description = "Desc",
        Format = TournamentFormat.SingleElimination,
        SeedingType = SeedingType.Standard,
        BracketReset = false,
    };

    [Fact]
    public async Task Execute_ReturnsTournament_WhenOwnerAndRealmAdmin()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Owner));
        _permissions.GetRoleForUserInRealm("user1", "realm1")
            .Returns(Result<RealmMemberRole, PermissionError>.Success(RealmMemberRole.Admin));
        _discriminatorGenerator.Generate().Returns("ABC1");
        _repo.GetByNameAndDiscriminatorAsync("Test Tournament", "ABC1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Failure(RepositoryError.NotFound));
        _idGenerator.Generate().Returns("t-id-1");
        _repo.AddAsync(Arg.Any<Tournament>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute(DefaultConfig(), "org1", "realm1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Equal("tournament_t-id-1", result.Value!.Id);
        Assert.Equal("Test Tournament", result.Value.Name);
        Assert.Equal("ABC1", result.Value.Discriminator);
        Assert.Equal(TournamentFormat.SingleElimination, result.Value.Format);
        Assert.Equal(SeedingType.Standard, result.Value.SeedingType);
        Assert.False(result.Value.BracketReset);
    }

    [Fact]
    public async Task Execute_ReturnsTournament_WhenManagerAndRealmAdmin()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Manager));
        _permissions.GetRoleForUserInRealm("user1", "realm1")
            .Returns(Result<RealmMemberRole, PermissionError>.Success(RealmMemberRole.Admin));
        _discriminatorGenerator.Generate().Returns("XYZ9");
        _repo.GetByNameAndDiscriminatorAsync("Test Tournament", "XYZ9", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Failure(RepositoryError.NotFound));
        _idGenerator.Generate().Returns("t-id-2");
        _repo.AddAsync(Arg.Any<Tournament>())
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
        _repo.GetByNameAndDiscriminatorAsync("Test Tournament", "ABC1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Failure(RepositoryError.DatabaseError));

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

        var existing = new Tournament { Id = "x", Name = "Test Tournament", Discriminator = "DUPE", TenantId = "tenant1" };
        _discriminatorGenerator.Generate().Returns("DUPE", "UNIQ");
        _repo.GetByNameAndDiscriminatorAsync("Test Tournament", "DUPE", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Success(existing));
        _repo.GetByNameAndDiscriminatorAsync("Test Tournament", "UNIQ", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Failure(RepositoryError.NotFound));
        _idGenerator.Generate().Returns("new-id");
        _repo.AddAsync(Arg.Any<Tournament>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute(DefaultConfig(), "org1", "realm1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        Assert.Equal("UNIQ", result.Value!.Discriminator);
        await _discriminatorGenerator.Received(2).Generate();
    }

    [Fact]
    public async Task Execute_ReturnsInternalError_WhenAddAsyncFails()
    {
        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Owner));
        _permissions.GetRoleForUserInRealm("user1", "realm1")
            .Returns(Result<RealmMemberRole, PermissionError>.Success(RealmMemberRole.Admin));
        _discriminatorGenerator.Generate().Returns("ABC1");
        _repo.GetByNameAndDiscriminatorAsync("Test Tournament", "ABC1", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Failure(RepositoryError.NotFound));
        _idGenerator.Generate().Returns("t-id-1");
        _repo.AddAsync(Arg.Any<Tournament>())
            .Returns(Result<Unit, RepositoryError>.Failure(RepositoryError.DatabaseError));

        var result = await CreateSut().Execute(DefaultConfig(), "org1", "realm1", "user1", "tenant1");

        Assert.True(result.IsFailure);
        Assert.Equal(AppError.InternalError, result.Error);
    }

    [Fact]
    public async Task Execute_SetsConfigFields_FromTournamentConfig()
    {
        var config = new TournamentConfig
        {
            Name = "My Tourney",
            Description = "A tournament",
            Format = TournamentFormat.DoubleElimination,
            SeedingType = SeedingType.Random,
            BracketReset = true,
            Game = "CS2",
            BestOf = 3,
            MapPool = new List<string> { "Dust2", "Mirage" },
        };

        _permissions.GetRoleForUserInOrganiser("user1", "org1")
            .Returns(Result<OrganiserMemberRole, PermissionError>.Success(OrganiserMemberRole.Owner));
        _permissions.GetRoleForUserInRealm("user1", "realm1")
            .Returns(Result<RealmMemberRole, PermissionError>.Success(RealmMemberRole.Admin));
        _discriminatorGenerator.Generate().Returns("DISC");
        _repo.GetByNameAndDiscriminatorAsync("My Tourney", "DISC", "tenant1")
            .Returns(Result<Tournament, RepositoryError>.Failure(RepositoryError.NotFound));
        _idGenerator.Generate().Returns("id1");
        _repo.AddAsync(Arg.Any<Tournament>())
            .Returns(Result<Unit, RepositoryError>.Success(Unit.Value));

        var result = await CreateSut().Execute(config, "org1", "realm1", "user1", "tenant1");

        Assert.True(result.IsSuccess);
        var t = result.Value!;
        Assert.Equal(TournamentFormat.DoubleElimination, t.Format);
        Assert.Equal(SeedingType.Random, t.SeedingType);
        Assert.True(t.BracketReset);
        Assert.Equal("CS2", t.Game);
        Assert.Equal(3, t.BestOf);
        Assert.Equal(new List<string> { "Dust2", "Mirage" }, t.MapPool);
        Assert.Equal("tenant1", t.TenantId);
    }
}
