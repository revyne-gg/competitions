namespace competitions.Domain.Competitions.Leagues.Models;

public enum OrganiserMemberRole
{
    Owner,
    Manager,
    Member,
    None
}

public static class OrganiserMemberRoleExtensions
{
    public static OrganiserMemberRole FromString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) 
        {
            return OrganiserMemberRole.None;
        }
        
        return Enum.TryParse<OrganiserMemberRole>(value, ignoreCase: true, out var result) ? 
            result : 
            OrganiserMemberRole.None;
    }
}