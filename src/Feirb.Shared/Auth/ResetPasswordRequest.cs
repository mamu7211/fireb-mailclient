using System.ComponentModel.DataAnnotations;

namespace Feirb.Shared.Auth;

public record ResetPasswordRequest(
    [Required] string Token,
    [Required, MinLength(8)] string NewPassword);
