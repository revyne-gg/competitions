using competitions.Application;
using competitions.Application.Ports;
using competitions.Application.UseCases;
using competitions.Domain.Competitions.Tournaments.Models;
using competitions.Shared;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Revyne.Services.Competitions.V1;
using Tournament = Revyne.Services.Competitions.V1.Tournament;
using TournamentRules = Revyne.Services.Competitions.V1.TournamentRules;

namespace competitions.Transport.Services;

public class CompetitionsService(
    CreateTournamentUseCase createTournamentUseCase,
    RegisterTeamForTournamentUseCase registerTeamForTournamentUseCase,
    UpdateTournamentRulesUseCase updateTournamentRulesUseCase,
    ITournamentRepository tournamentRepo
) : Revyne.Services.Competitions.V1.CompetitionsService.CompetitionsServiceBase
{
    public override async Task<Tournament> GetTournamentById(GetTournamentByIdRequest request, ServerCallContext context)
    {
        var res = await tournamentRepo.GetByIdAsync(request.TournamentId, request.TenantId);
        if (res.IsFailure)
            throw (res.Error == RepositoryError.NotFound
                ? new RpcException(new Status(StatusCode.NotFound, "Not found."))
                : new RpcException(new Status(StatusCode.Internal, "Internal error.")));
        return res.Value!.ToGrpc();
    }

    public override async Task<PaginatedTournamentRegistrations> GetTournamentRegistrations(
        GetTournamentRegistrationsRequest request, ServerCallContext context)
    {
        var page = request.Page > 0 ? (int)request.Page : 0;
        var pageSize = request.PageSize > 0 ? (int)request.PageSize : 20;

        var res = await tournamentRepo.GetRegistrationsAsync(request.TournamentId, request.TenantId, page, pageSize);
        if (res.IsFailure)
            throw new RpcException(new Status(StatusCode.Internal, "Internal error."));

        var (items, totalCount) = res.Value;
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var response = new PaginatedTournamentRegistrations
        {
            Pagination = new PaginationMeta
            {
                Page = (uint)page,
                TotalPages = (uint)totalPages,
                HasNextPage = page < totalPages - 1,
                HasPreviousPage = page > 0,
            },
        };
        response.Items.AddRange(items.Select(t => t.ToGrpc()));
        return response;
    }

    public override async Task<Tournament> CreateTournament(CreateTournamentRequest request, ServerCallContext context)
    {
        var tournamentConfig = new TournamentConfig
        {
            Name = request.Name,
            Description = request.Description,
            Format = request.Format.ToDomain(),
            Game = request.Game,
            BestOf = request.BestOf,
            MapPool = request.MapPool.Count > 0 ? request.MapPool.ToList() : null,
            SeedingType = request.SeedingType.ToDomain(),
            BracketReset = request.BracketReset,
            MaxParticipants = request.MaxParticipants,
            RegistrationType = request.RegistrationType.ToDomain(),
            RegistrationPassword = string.IsNullOrEmpty(request.RegistrationPassword) ? null : request.RegistrationPassword,
            Stages = request.Stages.Select(GrpcInputMapper.StageToDomain).ToList(),
        };
        
        var res = await createTournamentUseCase.Execute(
            tournamentConfig,
            request.OrganiserId,
            request.RealmId,
            request.UserId,
            request.TenantId
        );

        if (res.IsFailure)
        {
            throw res.Error!.Value.ToGrpcError();
        }
        
        return res.Value!.ToGrpc();
    }

    public override async Task<TournamentRegistration> RegisterTeamForTournament(
        RegisterTeamForTournamentRequest request, ServerCallContext context)
    {
        var res = await registerTeamForTournamentUseCase.Execute(
            request.TournamentId,
            request.TeamId,
            request.Password,
            request.UserId,
            request.TenantId
        );

        if (res.IsFailure)
            throw res.Error!.Value.ToGrpcError();

        return res.Value!.ToGrpc();
    }

    public override async Task<Google.Protobuf.WellKnownTypes.Empty> UnregisterTeamFromTournament(
        UnregisterTeamFromTournamentRequest request, ServerCallContext context)
    {
        var res = await tournamentRepo.RemoveRegistrationAsync(
            request.TournamentId, request.TeamId, request.TenantId);

        if (res.IsFailure)
            throw new RpcException(res.Error == RepositoryError.NotFound
                ? new Status(StatusCode.NotFound, "Registration not found.")
                : new Status(StatusCode.Internal, "Internal error."));

        return new Google.Protobuf.WellKnownTypes.Empty();
    }

    public override async Task<TournamentRules> GetTournamentRules(GetTournamentRulesRequest request, ServerCallContext context)
    {
        var res = await tournamentRepo.GetRulesAsync(request.TournamentId, request.TenantId);
        if (res.IsFailure)
            throw new RpcException(new Status(StatusCode.Internal, "Internal error."));

        var rules = res.Value!;
        return new TournamentRules
        {
            TournamentId = rules.TournamentId,
            Content = rules.Content,
            UpdatedAt = rules.UpdatedAt.ToTimestamp(),
        };
    }

    public override async Task<TournamentRules> UpdateTournamentRules(UpdateTournamentRulesRequest request, ServerCallContext context)
    {
        var res = await updateTournamentRulesUseCase.Execute(
            request.TournamentId,
            request.Content,
            request.OrganiserId,
            request.UserId,
            request.TenantId
        );

        if (res.IsFailure)
            throw res.Error!.Value.ToGrpcError();

        var rules = res.Value!;
        return new TournamentRules
        {
            TournamentId = rules.TournamentId,
            Content = rules.Content,
            UpdatedAt = rules.UpdatedAt.ToTimestamp(),
        };
    }
}