using System.ComponentModel.DataAnnotations;

namespace Feirb.Shared.Auth;

public record RequestResetRequest(
    [Required, EmailAddress] string Email);
