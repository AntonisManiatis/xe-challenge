using Microsoft.Extensions.DependencyInjection;

using Testcontainers.PostgreSql;

using Xe.Ratings.Infrastructure;

namespace Xe.Ratings.IntegrationTests;

public sealed class RatingsFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer container =
        new PostgreSqlBuilder().WithDatabase("xe_ratings").Build();

    private ServiceProvider? provider;

    public string? ConnectionString => container?.GetConnectionString();

    public IServiceScope CreateScope() => provider!.CreateScope();

    public async Task InitializeAsync()
    {
        await container.StartAsync();

        var assembly = typeof(RatingsServiceCollectionExtensions).Assembly;
        using var stream = assembly.GetManifestResourceStream("Xe.Ratings.Infrastructure.Data.init.sql");
        using var sr = new StreamReader(stream!);

        var scriptContent = await sr.ReadToEndAsync();

        await container.ExecScriptAsync(scriptContent);

        var connectionString = container.GetConnectionString();

        var services = new ServiceCollection();
        services.AddRatings(connectionString);

        provider = services.BuildServiceProvider(true);
    }

    public async Task DisposeAsync()
    {
        await provider!.DisposeAsync();

        await container.DisposeAsync();
    }
}