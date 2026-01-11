using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Api.Tests.FunctionalTests;
using API.DTOs;
using API.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace Api.Tests.PerformanceTests;

[Collection("Performance")]
public class MembersControllerPerformanceTests : PerformanceTestBase
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private string _authToken = string.Empty;
    private string _memberId = string.Empty;

    public MembersControllerPerformanceTests(ITestOutputHelper output) : base(output)
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        InitializeAsync().Wait();
    }

    private async Task InitializeAsync()
    {
        // Create and login a test user
        var registerDto = new RegisterDto
        {
            DisplayName = "MemberPerfTest",
            Email = "memberperf@test.com",
            Password = "Pa$$w0rd",
            Gender = "male",
            DateOfBirth = new DateOnly(1990, 1, 1),
            City = "TestCity",
            Country = "TestCountry"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/account/register", registerDto);
        var userDto = await registerResponse.Content.ReadFromJsonAsync<UserDto>();
        
        _authToken = userDto!.Token;
        _memberId = userDto.Id;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

        // Create some test members
        for (int i = 0; i < 20; i++)
        {
            var testUser = new RegisterDto
            {
                DisplayName = $"TestMember{i}",
                Email = $"testmember{i}@test.com",
                Password = "Pa$$w0rd",
                Gender = i % 2 == 0 ? "male" : "female",
                DateOfBirth = new DateOnly(1990 + i % 10, 1, 1),
                City = $"City{i}",
                Country = "TestCountry"
            };
            await _client.PostAsJsonAsync("/api/account/register", testUser);
        }
    }

    [Fact]
    public async Task GetMembers_Performance_ShouldHandleSequentialRequests()
    {
        // Arrange
        var iterations = 100;

        // Act
        var result = await MeasurePerformanceAsync(
            "Get Members List",
            async () =>
            {
                var response = await _client.GetAsync("/api/members?pageNumber=1&pageSize=10");
                response.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 200, maxP95Ms: 400);
    }

    [Fact]
    public async Task GetMembers_WithFilters_Performance()
    {
        // Arrange
        var iterations = 50;

        // Act
        var result = await MeasurePerformanceAsync(
            "Get Members with Filters",
            async () =>
            {
                var response = await _client.GetAsync(
                    "/api/members?pageNumber=1&pageSize=10&gender=female&minAge=18&maxAge=35&orderBy=created");
                response.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 250, maxP95Ms: 500);
    }

    [Fact]
    public async Task GetMembers_Performance_ConcurrentRequests()
    {
        // Act
        var result = await MeasureConcurrentPerformanceAsync(
            "Get Members Concurrent",
            async () =>
            {
                var client = _factory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var response = await client.GetAsync("/api/members?pageNumber=1&pageSize=10");
                response.EnsureSuccessStatusCode();
            },
            concurrentRequests: 30,
            iterations: 3);

        // Assert
        AssertPerformance(result, maxAverageMs: 400, maxP95Ms: 800, minSuccessRate: 0.90);
    }

    [Fact]
    public async Task GetMemberById_Performance()
    {
        // Arrange
        var iterations = 100;

        // Act
        var result = await MeasurePerformanceAsync(
            "Get Member By Id",
            async () =>
            {
                var response = await _client.GetAsync($"/api/members/{_memberId}");
                response.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 100, maxP95Ms: 200);
    }

    [Fact]
    public async Task GetMemberPhotos_Performance()
    {
        // Arrange
        var iterations = 100;

        // Act
        var result = await MeasurePerformanceAsync(
            "Get Member Photos",
            async () =>
            {
                var response = await _client.GetAsync($"/api/members/{_memberId}/photos");
                response.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 100, maxP95Ms: 200);
    }

    [Fact]
    public async Task UpdateMember_Performance()
    {
        // Arrange
        var iterations = 50;
        var counter = 0;

        // Act
        var result = await MeasurePerformanceAsync(
            "Update Member",
            async () =>
            {
                var updateDto = new MemberUpdateDto
                {
                    DisplayName = $"UpdatedName{Interlocked.Increment(ref counter)}",
                    Description = $"Updated description {counter}",
                    City = $"NewCity{counter}",
                    Country = "NewCountry"
                };

                var response = await _client.PutAsJsonAsync("/api/members", updateDto);
                response.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 300, maxP95Ms: 600);
    }

    [Fact]
    public async Task GetMembers_Pagination_Performance()
    {
        // Arrange
        var iterations = 30;
        var pageCounter = 0;

        // Act
        var result = await MeasurePerformanceAsync(
            "Get Members with Pagination",
            async () =>
            {
                var page = (Interlocked.Increment(ref pageCounter) % 3) + 1;
                var response = await _client.GetAsync($"/api/members?pageNumber={page}&pageSize=5");
                response.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 200, maxP95Ms: 400);
    }

    [Fact]
    public async Task GetMembers_LargePage_Performance()
    {
        // Arrange
        var iterations = 20;

        // Act
        var result = await MeasurePerformanceAsync(
            "Get Members Large Page",
            async () =>
            {
                var response = await _client.GetAsync("/api/members?pageNumber=1&pageSize=50");
                response.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 300, maxP95Ms: 600);
    }

    [Fact]
    public async Task GetMembers_UnderHighLoad()
    {
        // Act
        var result = await MeasureConcurrentPerformanceAsync(
            "Get Members High Load",
            async () =>
            {
                var client = _factory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                
                var page = Random.Shared.Next(1, 4);
                var pageSize = Random.Shared.Next(5, 20);
                
                var response = await client.GetAsync($"/api/members?pageNumber={page}&pageSize={pageSize}");
                response.EnsureSuccessStatusCode();
            },
            concurrentRequests: 50,
            iterations: 2);

        // Assert
        AssertPerformance(result, maxAverageMs: 600, maxP95Ms: 1200, minSuccessRate: 0.85);
    }

    [Fact]
    public async Task GetMember_NotFound_Performance()
    {
        // Arrange
        var iterations = 50;

        // Act
        var result = await MeasurePerformanceAsync(
            "Get Member Not Found",
            async () =>
            {
                var response = await _client.GetAsync("/api/members/nonexistent-id");
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 100, maxP95Ms: 200);
    }

    public override void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
        base.Dispose();
    }
}
