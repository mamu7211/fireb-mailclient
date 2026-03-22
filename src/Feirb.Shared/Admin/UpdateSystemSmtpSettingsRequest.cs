using System.ComponentModel.DataAnnotations;

namespace Feirb.Shared.Admin;

public record UpdateSystemSmtpSettingsRequest(
    [Required, StringLength(256)]
    string Host,
    [Required, Range(1, 65535)]
    int Port,
    bool UseTls,
    bool RequiresAuth,
    [StringLength(256)]
    string? Username,
    string? Password,
    [EmailAddress, StringLength(256)]
    string? FromAddress,
    [StringLength(256)]
    string? FromName);
