using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RecipeManager.Infrastructure.Persistence;

public class RecipeDbContextFactory : IDesignTimeDbContextFactory<RecipeDbContext>
{
    public RecipeDbContext CreateDbContext(string[] args)
    {
        // Mirror the API's configuration sources (and precedence) so migrations
        // resolve the same connection string the running app uses. User secrets
        // and environment variables override appsettings.json, exactly as at runtime.
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../RecipeManager.Api"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets("6d9fa2e0-5c27-482f-bb0d-7514222ed94d")
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<RecipeDbContext>();
        optionsBuilder.UseNpgsql(config.GetConnectionString("RecipeDb"));

        return new RecipeDbContext(optionsBuilder.Options);
    }
}
