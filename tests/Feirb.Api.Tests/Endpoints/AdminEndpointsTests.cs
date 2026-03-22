using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Feirb.Api.Data;
using Feirb.Shared.Admin;
using Feirb.Shared.Auth;
using Feirb.Shared.Setup;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Feirb.Api.Tests.Endpoints;

public class AdminEndpointsTests : IDisposable
{
    private readonly string _dbName = $"TestDb-{Guid.NewGuid()}";
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AdminEndpointsTests()
    {
        var dbName = _dbName;
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var dbDescriptors = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<FeirbDbContext>) ||
                    d.ServiceType.FullName?.Contains("FeirbDbContext") == true ||
                    d.ServiceType.FullName?.Contains("Npgsql") == true).ToList();
                foreach (var d in dbDescriptors)
                    services.Remove(d);

                services.AddDbContext<FeirbDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));
            });
        });
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetUsers_AsAdmin_ReturnsOkAsync()
    {
        var tokens = await SetupAndLoginAsAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var response = await _client.GetAsync("/api/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<AdminUserResponse>>();
        users.Should().NotBeNull();
        users.Should().ContainSingle(u => u.IsAdmin);
    }

    [Fact]
    public async Task GetUsers_AsNonAdmin_ReturnsForbiddenAsync()
    {
        // First create admin via setup so registration works
        await SetupAndLoginAsAdminAsync();

        // Register and login as non-admin user
        var registerRequest = new RegisterRequest("regularuser", "regular@example.com", "Password123!");
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("regularuser", "Password123!"));
        var tokens = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

        var response = await _client.GetAsync("/api/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_Unauthenticated_ReturnsUnauthorizedAsync()
    {
        var response = await _client.GetAsync("/api/admin/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<TokenResponse> SetupAndLoginAsAdminAsync()
    {
        var setupRequest = new CompleteSetupRequest(
            "admin", "admin@example.com", "AdminPassword123!",
            "smtp.example.com", 587, "smtp@example.com", "smtppass", true, true);
        await _client.PostAsJsonAsync("/api/setup/complete", setupRequest);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("admin", "AdminPassword123!"));
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var tokens = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
        tokens.Should().NotBeNull();
        return tokens!;
    }
}
