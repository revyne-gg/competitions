# competitions 🏆

The bit of [Revyne](https://revyne.gg) that runs the actual competitions: leagues, tournaments, brackets, standings, all the things that decide who gets bragging rights and who quietly uninstalls.

It is a .NET 10 gRPC microservice talking PostgreSQL and NATS, built the Clean Architecture way (Domain → Application → Infrastructure → Transport). Multi-tenant from top to bottom, so everything is scoped to a `tenantId` and nobody's league leaks into anybody else's. 🔒

## What it does

- **Leagues** with Divisions → Division Groups → Standings, soft delete and GDPR anonymisation included 🧹
- **Tournaments** in the formats people actually argue about: Single Elimination, Double Elimination, Swiss and Round Robin
- A pluggable engine per format (strategy pattern), coordinated by `TournamentEngine` so adding a new format does not mean rewriting the world
- Rules, registration types, supported games validation and match generation

`Competition` is the abstract base; `League` and `Tournament` are the two flavours. If you remember that, the rest of the codebase mostly explains itself. 🙂

## Getting started

You will need .NET 10 and a PostgreSQL instance. Local infra (Postgres, NATS and friends) lives in the platform `infrastructure/` compose stack.

```bash
dotnet build competitions.csproj     # build it
dotnet ef database update            # apply migrations
dotnet run                           # gRPC server on HTTP/2
dotnet test                          # run the tests
```

Or if you prefer it in a box 📦:

```bash
docker build -t competitions .
docker run -p 8080:8080 competitions
```

### Configuration

A handful of environment variables do the heavy lifting:

| Variable | What it is for |
|----------|----------------|
| `DATABASE_URL` | PostgreSQL connection string (required, the service refuses to start without it) |
| `NATS_URL` | NATS server for publishing domain events |
| `KETO_URL` / `KETO_ADMIN_URL` | Ory Keto for fine-grained permissions |
| `APP_PORT` | Defaults to `8080` |

## How it is built

A quick tour, in the order things flow:

- **Domain/** rich models with private constructors and factory methods, no external dependencies, business rules live here
- **Application/** use cases (`{Action}{Entity}UseCase`), DTOs and port interfaces. Everything returns `Result<V, E>`, we do not throw for business logic
- **Infrastructure/** EF Core entities, repositories, Keto and NATS clients, NanoID generation
- **Transport/** the gRPC handlers that the outside world actually calls

Contracts are shared as protobufs (compiled into the `gg.revyne.Contracts` NuGet package), IDs are NanoIDs with friendly prefixes like `league_…` and `tournament_…`, and errors are honest little enums rather than surprise exceptions.

There is a `CLAUDE.md` in the root with the deeper domain detail if you want the long version. 📚

## A note on conventions

- British spelling throughout, so it is `anonymisation`, not the other one 🇬🇧
- Always filter queries by `tenantId`. Always. We mean it
- Use the `Result<V, E>` pattern instead of throwing for anything that is a normal business outcome

## Part of something bigger

This service sits behind the Edge gateway alongside the other Revyne backend services (organisers, social, tenants, timeline, billing). On its own it is happy to run brackets all day, but it is most useful as one piece of the wider platform. 🎮
