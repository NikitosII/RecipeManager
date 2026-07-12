using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace RecipeManager.Api.IntegrationTests;

public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // A stable 32+ byte key for the test host. The real signing key lives in user secrets, which are not available on CI.
    private const string TestSigningKey = "integration-tests-signing-key-that-is-at-least-32-bytes";

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:17")
        .Build();

    public ApiFactory()
    {
        Environment.SetEnvironmentVariable("Jwt__SigningKey", TestSigningKey);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _database.StartAsync();
        // The container's connection string is only known after it starts.
        Environment.SetEnvironmentVariable("ConnectionStrings__RecipeDb", _database.GetConnectionString());
    }

    // Explicit implementation avoids clashing with the base ValueTask DisposeAsync.
    async Task IAsyncLifetime.DisposeAsync()
    {
        Environment.SetEnvironmentVariable("Jwt__SigningKey", null);
        Environment.SetEnvironmentVariable("ConnectionStrings__RecipeDb", null);
        await _database.DisposeAsync();
        await base.DisposeAsync();
    }
}

[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<ApiFactory>;
