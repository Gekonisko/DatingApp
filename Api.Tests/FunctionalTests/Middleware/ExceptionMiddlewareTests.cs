using API.Errors;
using API.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Api.Tests.FunctionalTests.Middleware;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_Should_Return_ApiException_When_Exception_Thrown()
    {
        // Arrange: create minimal host
        var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .Configure(app =>
                    {
                        app.UseMiddleware<ExceptionMiddleware>();

                        // Endpoint that always throws
                        app.Run(context => throw new InvalidOperationException("Test exception"));
                    })
                    .ConfigureLogging(logging => logging.ClearProviders());
            })
            .StartAsync();

        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/any-endpoint");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var json = await response.Content.ReadAsStringAsync();
        var apiException = JsonSerializer.Deserialize<ApiException>(json,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        Assert.NotNull(apiException);
        Assert.Equal(500, apiException.StatusCode);
        Assert.Equal("Test exception", apiException.Message);
        Assert.NotNull(apiException.Details); // Stack trace included in development
    }
}