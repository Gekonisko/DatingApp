using System.Net;
using System.Net.Http.Json;
using Api.Tests.FunctionalTests;
using API.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace Api.Tests.PerformanceTests;

[Collection("Performance")]
public class AccountControllerPerformanceTests : PerformanceTestBase
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AccountControllerPerformanceTests(ITestOutputHelper output) : base(output)
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_Performance_ShouldHandleSequentialRequests()
    {
        // Arrange
        var iterations = 50;
        var counter = 0;

        // Act
        var result = await MeasurePerformanceAsync(
            "Register User",
            async () =>
            {
                var registerDto = new RegisterDto
                {
                    DisplayName = $"TestUser{Interlocked.Increment(ref counter)}",
                    Email = $"testuser{counter}@test.com",
                    Password = "Pa$$w0rd",
                    Gender = "male",
                    DateOfBirth = new DateOnly(1990, 1, 1),
                    City = "TestCity",
                    Country = "TestCountry"
                };

                var response = await _client.PostAsJsonAsync("/api/account/register", registerDto);
                response.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 500, maxP95Ms: 1000);
    }

    [Fact]
    public async Task Login_Performance_ShouldHandleSequentialRequests()
    {
        // Arrange
        var email = "perf.test@test.com";
        var password = "Pa$$w0rd";

        // Create test user first
        var registerDto = new RegisterDto
        {
            DisplayName = "PerfTestUser",
            Email = email,
            Password = password,
            Gender = "male",
            DateOfBirth = new DateOnly(1990, 1, 1),
            City = "TestCity",
            Country = "TestCountry"
        };

        await _client.PostAsJsonAsync("/api/account/register", registerDto);

        var iterations = 100;

        // Act
        var result = await MeasurePerformanceAsync(
            "Login User",
            async () =>
            {
                var loginDto = new LoginDto
                {
                    Email = email,
                    Password = password
                };

                var response = await _client.PostAsJsonAsync("/api/account/login", loginDto);
                response.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 300, maxP95Ms: 600);
    }

    [Fact]
    public async Task Login_Performance_ShouldHandleConcurrentRequests()
    {
        // Arrange
        var email = "perf.concurrent@test.com";
        var password = "Pa$$w0rd";

        // Create test user first
        var registerDto = new RegisterDto
        {
            DisplayName = "PerfConcurrentUser",
            Email = email,
            Password = password,
            Gender = "male",
            DateOfBirth = new DateOnly(1990, 1, 1),
            City = "TestCity",
            Country = "TestCountry"
        };

        await _client.PostAsJsonAsync("/api/account/register", registerDto);

        // Act
        var result = await MeasureConcurrentPerformanceAsync(
            "Login User Concurrent",
            async () =>
            {
                var client = _factory.CreateClient();
                var loginDto = new LoginDto
                {
                    Email = email,
                    Password = password
                };

                var response = await client.PostAsJsonAsync("/api/account/login", loginDto);
                response.EnsureSuccessStatusCode();
            },
            concurrentRequests: 20,
            iterations: 5);

        // Assert
        AssertPerformance(result, maxAverageMs: 500, maxP95Ms: 1000, minSuccessRate: 0.90);
    }

    [Fact]
    public async Task Register_Performance_UnderLoad()
    {
        // Arrange
        var counter = 10000;

        // Act
        var result = await MeasureConcurrentPerformanceAsync(
            "Register User Under Load",
            async () =>
            {
                var client = _factory.CreateClient();
                var id = Interlocked.Increment(ref counter);
                var registerDto = new RegisterDto
                {
                    DisplayName = $"LoadTestUser{id}",
                    Email = $"loadtest{id}@test.com",
                    Password = "Pa$$w0rd",
                    Gender = id % 2 == 0 ? "male" : "female",
                    DateOfBirth = new DateOnly(1990, 1, 1),
                    City = "TestCity",
                    Country = "TestCountry"
                };

                var response = await client.PostAsJsonAsync("/api/account/register", registerDto);
                response.EnsureSuccessStatusCode();
            },
            concurrentRequests: 25,
            iterations: 4);

        // Assert
        AssertPerformance(result, maxAverageMs: 800, maxP95Ms: 1500, minSuccessRate: 0.85);
    }

    [Fact]
    public async Task Login_InvalidCredentials_Performance()
    {
        // Arrange
        var iterations = 50;

        // Act
        var result = await MeasurePerformanceAsync(
            "Login with Invalid Credentials",
            async () =>
            {
                var loginDto = new LoginDto
                {
                    Email = "invalid@test.com",
                    Password = "WrongPassword"
                };

                var response = await _client.PostAsJsonAsync("/api/account/login", loginDto);
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 200, maxP95Ms: 400);
    }

    public override void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
        base.Dispose();
    }
}
