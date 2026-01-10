using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Api.Tests.IntegrationTests.SignalR;

public class SignalRWebApplicationFactory
    : CustomWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        // Enable console logging for test server to surface server-side exceptions during tests
        builder.ConfigureLogging(logging => logging.AddConsole());
        base.ConfigureWebHost(builder);
    }
}