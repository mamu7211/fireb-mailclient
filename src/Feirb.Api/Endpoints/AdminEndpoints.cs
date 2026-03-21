using Feirb.Api.Data;
using Feirb.Shared.Admin;
using Microsoft.EntityFrameworkCore;

namespace Feirb.Api.Endpoints;

public static class AdminEndpoints
{
    public static RouteGroupBuilder MapAdminEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/users", GetUsersAsync);
        return group;
    }

    private static async Task<IResult> GetUsersAsync(FeirbDbContext db) =>
        Results.Ok(await db.Users
            .Select(u => new AdminUserResponse(u.Id, u.Username, u.Email, u.IsAdmin, u.CreatedAt))
            .ToListAsync());
}
