namespace competitions.Domain.Competitions.Leagues.Models;

public enum TeamMemberRole
{
    Captain,
    Member,
    None
}

public static class TeamMemberRoleExtensions
{
    public static TeamMemberRole FromString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return TeamMemberRole.None;
        }

        return Enum.TryParse<TeamMemberRole>(value, ignoreCase: true, out var result) ?
            result :
            TeamMemberRole.None;
    }
}
