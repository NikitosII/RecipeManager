using System.Net;
using System.Net.Http.Json;
using RecipeManager.Application.DTOs;

namespace RecipeManager.Api.IntegrationTests;

[Collection("Api")]
public class AuthFlowTests(ApiFactory factory)
{
    [Fact]
    public async Task Register_ReturnsTokensAndUser()
    {
        var client = factory.CreateClient();

        var auth = await client.RegisterUserAsync();

        Assert.False(string.IsNullOrWhiteSpace(auth.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(auth.RefreshToken));
        Assert.NotEqual(Guid.Empty, auth.UserId);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Succeeds()
    {
        var client = factory.CreateClient();
        var email = $"login-{Guid.NewGuid():N}@example.com";
        await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            firstName = "Log",
            lastName = "In",
            email,
            password = "Password1"
        });

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Password1" });

        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(auth);
        Assert.Equal(email, auth!.Email);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();
        var email = $"bad-{Guid.NewGuid():N}@example.com";
        await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            firstName = "Bad",
            lastName = "Pass",
            email,
            password = "Password1"
        });

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "WrongPassword9" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_RotatesTheRefreshToken()
    {
        var client = factory.CreateClient();
        var auth = await client.RegisterUserAsync();

        var response = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = auth.RefreshToken });

        response.EnsureSuccessStatusCode();
        var refreshed = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(refreshed);
        Assert.NotEqual(auth.RefreshToken, refreshed!.RefreshToken);

        // The rotated-out token must no longer be accepted.
        var reuse = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = auth.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesTheRefreshToken()
    {
        var client = factory.CreateClient();
        var auth = await client.RegisterUserAsync();

        var logout = await client.PostAsJsonAsync("/api/v1/auth/logout", new { refreshToken = auth.RefreshToken });
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var refresh = await client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken = auth.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }
}
