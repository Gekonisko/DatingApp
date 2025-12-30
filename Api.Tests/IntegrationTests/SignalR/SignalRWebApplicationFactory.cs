using Microsoft.AspNetCore.Hosting;

namespace Api.Tests.IntegrationTests.SignalR;

public class SignalRWebApplicationFactory
    : CustomWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        base.ConfigureWebHost(builder);
    }
}