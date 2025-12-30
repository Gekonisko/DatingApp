using Testcontainers.PostgreSql;

namespace API.IntegrationTests;

public sealed class PostgresTestContainer : IAsyncLifetime
{
    private readonly PostgreSqlContainer container;

    public PostgreSqlContainer GetContainer()
    {
        return container;
    }

    public string ConnectionString => GetContainer().GetConnectionString();

    public PostgresTestContainer()
    {
        container = new PostgreSqlBuilder()
            .WithDatabase(new PostgreSqlConfiguration("testdb", "postgres", "postgres").Database)
            .WithImage("postgres:15-alpine")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
        => await GetContainer().StartAsync();

    public async Task DisposeAsync()
        => await GetContainer().DisposeAsync();
}