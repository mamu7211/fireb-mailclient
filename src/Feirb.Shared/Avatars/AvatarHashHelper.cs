using System.Text;

namespace Feirb.Shared.Avatars;

public static class AvatarHashHelper
{
    public static string ComputeEmailHash(string email)
    {
        ArgumentNullException.ThrowIfNull(email);

        var normalized = email.Trim().ToLowerInvariant();
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(normalized))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
