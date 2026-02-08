# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A competitions management microservice (leagues & tournaments) built with C# / .NET 10.0, gRPC, PostgreSQL (EF Core), and NATS messaging. Part of the larger "brackzer" backend ecosystem.

## Development Environment (Windows)

- **OS**: Windows with Git Bash (required for Claude Code).
- **Output Redirection**: NEVER use `> nul`. Always use `> /dev/null` for output suppression to avoid creating physical "nul" files that break Git.
- **Pathing**: Use Unix-style forward slashes `/` in commands.

## Build & Run Commands

```bash
dotnet build competitions.csproj          # Build
dotnet run                                # Run (gRPC on HTTP/2)
dotnet test                               # Run tests (no test project yet)
```

Docker:
```bash
docker build -t competitions .
docker run -p 8080:8080 competitions
```

Database: PostgreSQL at `127.0.0.1:5432`, database `competitions`, user `brackzr`.

## Architecture

Clean Architecture with DDD, using Ports & Adapters pattern:

- **Domain/** — Rich domain models with business logic, private constructors, factory methods. No external dependencies.
- **Application/** — Use cases (`{Action}{Entity}UseCase`), DTOs, port interfaces (repositories, services). All use cases return `Task<Result<V, E>>`.
- **Infrastructure/** — EF Core entities, repository implementations, permission service (Ory Keto), NanoID generator. Two-way mapping between entities and domain models using `Activator.CreateInstance` for private constructors.
- **Transport/** — gRPC service implementations.
- **Shared/** — `Result<V, E>` type (success/failure with implicit conversions), `Error<T>`, `Unit`.

## Key Domain Concepts

**Competition** is the abstract base for **League** and **Tournament**. Tournaments support multiple formats via strategy pattern: `SingleElimination`, `DoubleElimination`, `Swiss`, `RoundRobin` — each with a dedicated engine class implementing `ITournamentEngine`. `TournamentEngine` coordinates by selecting the appropriate engine based on format.

Leagues have Divisions → DivisionGroups → Standings. All entities are multi-tenant (`TenantId` on everything). Leagues support soft delete with GDPR anonymisation.

## Conventions

- **IDs**: NanoID with prefix — `league_{id}`, `tournament_{id}`
- **Error handling**: `Result<V, E>` pattern, never exceptions for business logic. Error enums: `AppError`, `RepositoryError`, `CompetitionError`, `PermissionError`
- **Namespaces**: `competitions.Domain.*`, `competitions.Application.*`, `competitions.Infrastructure.*`, `competitions.Services.*`, `competitions.Shared.*`
- **Naming**: Use cases `{Action}{Entity}UseCase`, repositories `{Entity}Repository`, entities `{Name}Entity`, engines `{Format}Engine`
- **Protos**: Package `revyne.{services|events}.{domain}.v1`, snake_case fields. Defined in `protos/` and compiled via .csproj `<Protobuf>` items
- **Mappers**: `Application.Mapper` (domain→DTO), `Infrastructure.Mapper` (entity↔domain), both use extension methods
- **Repository queries**: Always filter by `tenantId`. Signature pattern: `GetByIdAsync(string id, string tenantId)`
- **Domain model protection**: Private constructors, public factory methods with validation, permission checks in domain (e.g., `CanBeDeletedBy`)
- **EF Core**: Table-per-Hierarchy with `CompetitionType` discriminator, separate config tables for league/tournament-specific data
