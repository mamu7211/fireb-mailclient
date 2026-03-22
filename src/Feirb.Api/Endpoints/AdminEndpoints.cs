using System.Security.Claims;
using Feirb.Api.Data;
using Feirb.Api.Data.Entities;
using Feirb.Api.Resources;
using Feirb.Api.Services;
using Feirb.Shared.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Feirb.Api.Endpoints;

public static class AdminEndpoints
{
    public static RouteGroupBuilder MapAdminEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/users", GetUsersAsync);
        group.MapPost("/users", CreateUserAsync);
        group.MapPut("/users/{id:guid}", UpdateUserAsync);
        group.MapDelete("/users/{id:guid}", DeleteUserAsync);
        group.MapPost("/users/{id:guid}/reset-password", ResetPasswordAsync);
        return group;
    }

    private static async Task<IResult> GetUsersAsync(FeirbDbContext db) =>
        Results.Ok(await db.Users
            .Select(u => new AdminUserResponse(u.Id, u.Username, u.Email, u.IsAdmin, u.CreatedAt))
            .ToListAsync());

    private static async Task<IResult> CreateUserAsync(
        CreateUserRequest request,
        FeirbDbContext db,
        IAuthService authService,
        IStringLocalizer<ApiMessages> localizer)
    {
        if (await db.Users.AnyAsync(u => u.Username == request.Username))
            return Results.Conflict(new { message = localizer["UsernameAlreadyTaken"].Value });

        if (await db.Users.AnyAsync(u => u.Email == request.Email))
            return Results.Conflict(new { message = localizer["EmailAlreadyRegistered"].Value });

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = authService.HashPassword(request.Password),
            IsAdmin = request.IsAdmin,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var response = new AdminUserResponse(user.Id, user.Username, user.Email, user.IsAdmin, user.CreatedAt);
        return Results.Created($"/api/admin/users/{user.Id}", response);
    }

    private static async Task<IResult> UpdateUserAsync(
        Guid id,
        UpdateUserRequest request,
        HttpContext httpContext,
        FeirbDbContext db,
        IStringLocalizer<ApiMessages> localizer)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
            return Results.NotFound(new { message = localizer["UserNotFound"].Value });

        var currentUserId = GetCurrentUserId(httpContext);

        if (user.Id == currentUserId && !request.IsAdmin)
            return Results.BadRequest(new { message = localizer["CannotDemoteSelf"].Value });

        if (!request.IsAdmin && user.IsAdmin)
        {
            var adminCount = await db.Users.CountAsync(u => u.IsAdmin);
            if (adminCount <= 1)
                return Results.BadRequest(new { message = localizer["LastAdminCannotBeDemoted"].Value });
        }

        user.Email = request.Email;
        user.IsAdmin = request.IsAdmin;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Results.Ok(new AdminUserResponse(user.Id, user.Username, user.Email, user.IsAdmin, user.CreatedAt));
    }

    private static async Task<IResult> DeleteUserAsync(
        Guid id,
        HttpContext httpContext,
        FeirbDbContext db,
        IStringLocalizer<ApiMessages> localizer)
    {
        var currentUserId = GetCurrentUserId(httpContext);

        if (id == currentUserId)
            return Results.BadRequest(new { message = localizer["CannotDeleteSelf"].Value });

        var user = await db.Users.FindAsync(id);
        if (user is null)
            return Results.NotFound(new { message = localizer["UserNotFound"].Value });

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        return Results.Ok(new { message = localizer["UserDeleted"].Value });
    }

    private static async Task<IResult> ResetPasswordAsync(
        Guid id,
        HttpContext httpContext,
        FeirbDbContext db,
        IAuthService authService,
        IStringLocalizer<ApiMessages> localizer)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
            return Results.NotFound(new { message = localizer["UserNotFound"].Value });

        var token = authService.GenerateResetToken();

        db.PasswordResetTokens.Add(new Data.Entities.PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var request = httpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";
        var resetLink = $"{baseUrl}/reset-password/{token}";

        return Results.Ok(new ResetPasswordResponse(token, resetLink));
    }

    private static Guid GetCurrentUserId(HttpContext httpContext) =>
        Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}
