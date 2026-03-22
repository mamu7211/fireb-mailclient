using System.ComponentModel.DataAnnotations;

namespace Feirb.Shared.Admin;

public record UpdateUserRequest(
    [Required, EmailAddress, StringLength(256)]
    string Email,
    bool IsAdmin);
