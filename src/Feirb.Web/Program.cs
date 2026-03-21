using System.Globalization;
using Feirb.Web;
using Feirb.Web.Http;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddLocalization();

builder.Services.AddTransient<CultureDelegatingHandler>();
builder.Services.AddHttpClient("FeirbApi", client =>
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<CultureDelegatingHandler>();
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("FeirbApi"));

var host = builder.Build();

var jsInterop = host.Services.GetRequiredService<IJSRuntime>();
var storedCulture = await jsInterop.InvokeAsync<string?>("blazorCulture.get");
var cultureName = storedCulture ?? CultureInfo.CurrentCulture.Name;
if (string.IsNullOrEmpty(cultureName))
    cultureName = "en-US";

var culture = new CultureInfo(cultureName);
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await host.RunAsync();
