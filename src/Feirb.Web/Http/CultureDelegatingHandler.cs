using System.Globalization;

namespace Feirb.Web.Http;

public sealed class CultureDelegatingHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.Headers.AcceptLanguage.Clear();
        request.Headers.AcceptLanguage.ParseAdd(CultureInfo.CurrentUICulture.Name);

        return base.SendAsync(request, cancellationToken);
    }
}
