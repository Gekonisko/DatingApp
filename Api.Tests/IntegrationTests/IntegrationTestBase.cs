using API.Data;
using API.IntegrationTests;
using Microsoft.EntityFrameworkCore;

namespace Api.Tests.IntegrationTests;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgresTestContainer _postgres = new();

    protected AppDbContext Context = null!;

    public async Task InitializeAsync()
    {
        await _postgres.InitializeAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.ConnectionString)
            .Options;

        Context = new AppDbContext(options);
        // Use EnsureCreated for the transient test container database to avoid
        // requiring migrations to be present in the test project.
        await Context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    // Factory methods (flexible)
    protected LikesRepository CreateLikesRepository()
        => new(Context);

    protected MemberRepository CreateMemberRepository()
        => new(Context);
}