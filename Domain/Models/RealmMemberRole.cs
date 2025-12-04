namespace leagues.Domain.Models;

public enum RealmMemberRole
{
    Admin,
    Moderator,
    Caster,
    Journalist,
    None
}

public static class RealmMemberRoleExtensions
{
    public static RealmMemberRole FromString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) {
            return RealmMemberRole.None;
        }
        
        return Enum.TryParse<RealmMemberRole>(value, ignoreCase: true, out var result) ? 
            result : 
            RealmMemberRole.None;
    }
}