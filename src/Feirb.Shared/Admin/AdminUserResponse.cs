namespace Feirb.Shared.Admin;

public record AdminUserResponse(Guid Id, string Username, string Email, bool IsAdmin, DateTime CreatedAt);
