using API.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections;
using System.Net;
using System.Net.Http.Json;

namespace Api.Tests.IntegrationTests.SignalR;

public abstract class SignalRTestBase
    : IClassFixture<SignalRWebApplicationFactory>
{
    protected readonly SignalRWebApplicationFactory Factory;

    protected SignalRTestBase(SignalRWebApplicationFactory factory)
    {
        Factory = factory;
    }

    protected HubConnection CreateConnection(string token, string userId)
    {
        var hubUrl = new Uri(Factory.Server.BaseAddress, $"hubs/messages?userId={userId}");

        return new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(token);
                // Force LongPolling (TestServer doesn't support WebSockets) and
                // route transport HTTP requests to the test server handler
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .WithAutomaticReconnect()
            .Build();
    }

    protected async Task<(string Token, string UserId)> RegisterAndLoginAsync(string email)
    {
        var client = Factory.CreateClient();

        var register = new RegisterDto
        {
            DisplayName = "User",
            Email = email,
            Password = "Pa$$w0rd!",
            Gender = "male",
            City = "City",
            Country = "Country",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25))
        };

        var registerResponse = await client.PostAsJsonAsync("/api/account/register", register);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var login = new LoginDto
        {
            Email = email,
            Password = register.Password
        };

        var loginResponse = await client.PostAsJsonAsync("/api/account/login", login);
        var user = await loginResponse.Content.ReadFromJsonAsync<UserDto>();

        return (user!.Token, user.Id);
    }
}