using competitions.Application;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Leagues.Models;
using competitions.Domain.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Revyne.Services.Competitions.V1;
using GrpcLeague = Revyne.Services.Competitions.V1.League;
using GrpcDivision = Revyne.Services.Competitions.V1.Division;
using GrpcDivisionGroup = Revyne.Services.Competitions.V1.DivisionGroup;

namespace competitions.Transport.Services;

public class LeaguesService(
    CreateLeagueUseCase createLeagueUseCase,
    CreateDivisionUseCase createDivisionUseCase,
    CreateDivisionGroupUseCase createDivisionGroupUseCase,
    AddTeamToLeagueUseCase addTeamToDivisionGroupUseCase,
    RegisterTeamForLeagueUseCase registerTeamUseCase,
    UnregisterTeamFromLeagueUseCase unregisterTeamUseCase,
    competitions.Application.Ports.ILeagueRepository leagueRepo
) : Revyne.Services.Competitions.V1.LeaguesService.LeaguesServiceBase
{
    // ── League ─────────────────────────────────────────────────────────────────

    public override async Task<GrpcLeague> CreateLeague(CreateLeagueRequest request, ServerCallContext context)
    {
        var config = new LeagueConfig
        {
            Name = request.Name,
            Description = request.Description,
            RealmId = request.RealmId,
            TenantId = request.TenantId,
            Legs = request.Legs.ToDomain(),
            Game = request.Game,
            BestOf = request.BestOf,
            MapPool = request.MapPool.Count > 0 ? request.MapPool.ToList() : null,
            RegistrationPeriodStart = request.RegistrationPeriodStart?.ToDateTime(),
            RegistrationPeriodEnd   = request.RegistrationPeriodEnd?.ToDateTime(),
            LeaguePeriodStart       = request.LeaguePeriodStart?.ToDateTime(),
            LeaguePeriodEnd         = request.LeaguePeriodEnd?.ToDateTime(),
        };

        var res = await createLeagueUseCase.Execute(
            config,
            request.OrganiserId,
            request.RealmId,
            request.UserId,
            request.TenantId
        );

        if (res.IsFailure) throw res.Error!.Value.ToGrpcError();
        return res.Value!.ToGrpc();
    }

    public override async Task<GrpcLeague> GetLeagueById(GetLeagueByIdRequest request, ServerCallContext context)
    {
        var res = await leagueRepo.GetByIdAsync(request.LeagueId, request.TenantId);
        if (res.IsFailure)
            throw (res.Error == RepositoryError.NotFound
                ? new RpcException(new Status(StatusCode.NotFound, "Not found."))
                : new RpcException(new Status(StatusCode.Internal, "Internal error.")));
        return res.Value!.ToGrpc();
    }

    public override async Task<GrpcLeague> UpdateLeague(UpdateLeagueRequest request, ServerCallContext context)
    {
        var leagueRes = await leagueRepo.GetByIdAsync(request.LeagueId, request.TenantId);
        if (leagueRes.IsFailure)
            throw (leagueRes.Error == RepositoryError.NotFound
                ? new RpcException(new Status(StatusCode.NotFound, "Not found."))
                : new RpcException(new Status(StatusCode.Internal, "Internal error.")));

        var league = leagueRes.Value!;

        if (request.HasName) league.Name = request.Name;
        if (request.HasDescription) league.Description = request.Description;
        if (request.HasState) league.State = request.State.ToDomain();
        if (request.HasLegs) league.Legs = request.Legs.ToDomain();
        if (request.HasGame) league.Game = request.Game;
        if (request.HasBestOf) league.BestOf = request.BestOf;
        if (request.HasMapPool) league.MapPool = request.MapPool.Count > 0 ? request.MapPool.ToList() : null;
        if (request.RegistrationPeriodStart != null) league.RegistrationPeriodStart = request.RegistrationPeriodStart.ToDateTime();
        if (request.RegistrationPeriodEnd != null) league.RegistrationPeriodEnd = request.RegistrationPeriodEnd.ToDateTime();
        if (request.LeaguePeriodStart != null) league.LeaguePeriodStart = request.LeaguePeriodStart.ToDateTime();
        if (request.LeaguePeriodEnd != null) league.LeaguePeriodEnd = request.LeaguePeriodEnd.ToDateTime();

        var updateRes = await leagueRepo.Update(league);
        if (updateRes.IsFailure) throw new RpcException(new Status(StatusCode.Internal, "Internal error."));

        return league.ToGrpc();
    }

    public override async Task<Empty> DeleteLeague(DeleteLeagueRequest request, ServerCallContext context)
    {
        var leagueRes = await leagueRepo.GetByIdAsync(request.LeagueId, request.TenantId);
        if (leagueRes.IsFailure)
            throw (leagueRes.Error == RepositoryError.NotFound
                ? new RpcException(new Status(StatusCode.NotFound, "Not found."))
                : new RpcException(new Status(StatusCode.Internal, "Internal error.")));

        var league = leagueRes.Value!;
        league.Anonymise(request.UserId);

        var updateRes = await leagueRepo.Update(league);
        if (updateRes.IsFailure) throw new RpcException(new Status(StatusCode.Internal, "Internal error."));

        return new Empty();
    }

    // ── Divisions ──────────────────────────────────────────────────────────────

    public override async Task<Revyne.Services.Competitions.V1.Division> CreateDivision(
        CreateDivisionRequest request, ServerCallContext context)
    {
        var res = await createDivisionUseCase.Execute(
            request.LeagueId,
            request.Name,
            request.Order,
            request.BestOf,
            request.MaxTeamsPerGroup,
            request.Tag,
            request.OrganiserId,
            request.UserId,
            request.TenantId
        );

        if (res.IsFailure) throw res.Error!.Value.ToGrpcError();
        return res.Value!.ToGrpc();
    }

    public override async Task<PaginatedDivisions> GetDivisionsByLeague(
        GetDivisionsByLeagueRequest request, ServerCallContext context)
    {
        var res = await leagueRepo.GetDivisionsByLeagueAsync(request.LeagueId, request.TenantId);
        if (res.IsFailure) throw new RpcException(new Status(StatusCode.Internal, "Internal error."));

        var response = new PaginatedDivisions
        {
            Pagination = new PaginationMeta { Page = 1, TotalPages = 1, HasNextPage = false, HasPreviousPage = false }
        };
        response.Items.AddRange(res.Value!.Select(d => d.ToGrpc()));
        return response;
    }

    public override async Task<Revyne.Services.Competitions.V1.Division> GetDivisionById(
        GetDivisionByIdRequest request, ServerCallContext context)
    {
        var res = await leagueRepo.GetDivisionByIdAsync(request.DivisionId, request.TenantId);
        if (res.IsFailure)
            throw (res.Error == RepositoryError.NotFound
                ? new RpcException(new Status(StatusCode.NotFound, "Not found."))
                : new RpcException(new Status(StatusCode.Internal, "Internal error.")));
        return res.Value!.ToGrpc();
    }

    public override async Task<Empty> DeleteDivision(DeleteDivisionRequest request, ServerCallContext context)
    {
        var res = await leagueRepo.DeleteDivisionAsync(request.DivisionId, request.TenantId);
        if (res.IsFailure)
            throw (res.Error == RepositoryError.NotFound
                ? new RpcException(new Status(StatusCode.NotFound, "Not found."))
                : new RpcException(new Status(StatusCode.Internal, "Internal error.")));
        return new Empty();
    }

    // ── Division Groups ────────────────────────────────────────────────────────

    public override async Task<Revyne.Services.Competitions.V1.DivisionGroup> CreateDivisionGroup(
        CreateDivisionGroupRequest request, ServerCallContext context)
    {
        var res = await createDivisionGroupUseCase.Execute(
            request.DivisionId,
            request.Name,
            request.Order,
            request.OrganiserId,
            request.UserId,
            request.TenantId
        );

        if (res.IsFailure) throw res.Error!.Value.ToGrpcError();
        return res.Value!.ToGrpc();
    }

    public override async Task<PaginatedDivisionGroups> GetDivisionGroupsByDivision(
        GetDivisionGroupsByDivisionRequest request, ServerCallContext context)
    {
        var res = await leagueRepo.GetDivisionGroupsByDivisionAsync(request.DivisionId, request.TenantId);
        if (res.IsFailure) throw new RpcException(new Status(StatusCode.Internal, "Internal error."));

        var response = new PaginatedDivisionGroups
        {
            Pagination = new PaginationMeta { Page = 1, TotalPages = 1, HasNextPage = false, HasPreviousPage = false }
        };
        response.Items.AddRange(res.Value!.Select(g => g.ToGrpc()));
        return response;
    }

    public override async Task<Revyne.Services.Competitions.V1.DivisionGroup> GetDivisionGroupById(
        GetDivisionGroupByIdRequest request, ServerCallContext context)
    {
        var res = await leagueRepo.GetDivisionGroupByIdAsync(request.GroupId, request.TenantId);
        if (res.IsFailure)
            throw (res.Error == RepositoryError.NotFound
                ? new RpcException(new Status(StatusCode.NotFound, "Not found."))
                : new RpcException(new Status(StatusCode.Internal, "Internal error.")));
        return res.Value!.ToGrpc();
    }

    public override async Task<Empty> DeleteDivisionGroup(DeleteDivisionGroupRequest request, ServerCallContext context)
    {
        var res = await leagueRepo.DeleteDivisionGroupAsync(request.GroupId, request.TenantId);
        if (res.IsFailure)
            throw (res.Error == RepositoryError.NotFound
                ? new RpcException(new Status(StatusCode.NotFound, "Not found."))
                : new RpcException(new Status(StatusCode.Internal, "Internal error.")));
        return new Empty();
    }

    public override async Task<Revyne.Services.Competitions.V1.DivisionGroup> AddTeamToDivisionGroup(
        AddTeamToDivisionGroupRequest request, ServerCallContext context)
    {
        var res = await addTeamToDivisionGroupUseCase.Execute(
            request.GroupId,
            request.TeamId,
            request.UserId,
            request.OrganiserId,
            request.TenantId
        );

        if (res.IsFailure) throw res.Error!.Value.ToGrpcError();
        return res.Value!.ToGrpc();
    }

    // ── Registrations ──────────────────────────────────────────────────────────

    public override async Task<LeagueRegistration> RegisterTeamForLeague(
        RegisterTeamRequest request, ServerCallContext context)
    {
        var res = await registerTeamUseCase.Execute(
            request.LeagueId,
            request.TeamId,
            request.UserId,
            request.TenantId
        );

        if (res.IsFailure) throw res.Error!.Value.ToGrpcError();
        return res.Value!.ToGrpc();
    }

    public override async Task<Empty> UnregisterTeamFromLeague(
        UnregisterTeamRequest request, ServerCallContext context)
    {
        var res = await unregisterTeamUseCase.Execute(
            request.LeagueId,
            request.TeamId,
            request.UserId,
            request.TenantId
        );

        if (res.IsFailure) throw res.Error!.Value.ToGrpcError();
        return new Empty();
    }

    public override async Task<PaginatedLeagueRegistrations> GetLeagueRegistrations(
        GetLeagueRegistrationsRequest request, ServerCallContext context)
    {
        var res = await leagueRepo.GetRegistrationsAsync(request.LeagueId, request.TenantId);
        if (res.IsFailure) throw new RpcException(new Status(StatusCode.Internal, "Internal error."));

        var response = new PaginatedLeagueRegistrations
        {
            Pagination = new PaginationMeta { Page = 1, TotalPages = 1, HasNextPage = false, HasPreviousPage = false }
        };
        response.Items.AddRange(res.Value!.Select(r => r.ToGrpc()));
        return response;
    }

    public override async Task<LeagueRegistration> GetTeamRegistrationStatus(
        GetTeamRegistrationStatusRequest request, ServerCallContext context)
    {
        var res = await leagueRepo.GetRegistrationAsync(request.LeagueId, request.TeamId, request.TenantId);
        if (res.IsFailure)
            throw (res.Error == RepositoryError.NotFound
                ? new RpcException(new Status(StatusCode.NotFound, "Not found."))
                : new RpcException(new Status(StatusCode.Internal, "Internal error.")));
        return res.Value!.ToGrpc();
    }
}
