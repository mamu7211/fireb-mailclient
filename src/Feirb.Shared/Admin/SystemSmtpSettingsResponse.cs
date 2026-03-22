namespace Feirb.Shared.Admin;

public record SystemSmtpSettingsResponse(
    string Host,
    int Port,
    bool UseTls,
    bool RequiresAuth,
    string? Username,
    string? FromAddress,
    string? FromName);
