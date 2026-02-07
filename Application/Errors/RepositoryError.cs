namespace competitions.Application;

public enum RepositoryError
{
    DatabaseError = 1,
    DatabaseConcurrencyError = 2,
    DuplicateLeague = 2,
    NoPermission = 3,
    InternalError = 4
}