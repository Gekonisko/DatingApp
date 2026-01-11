using Api.Tests.FunctionalTests;
using API.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace Api.Tests.PerformanceTests;

[Collection("Performance")]
public class LikesControllerPerformanceTests : PerformanceTestBase
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private string _authToken = string.Empty;
    private string _memberId = string.Empty;
    private readonly List<string> _targetMemberIds = new();

    public LikesControllerPerformanceTests(ITestOutputHelper output) : base(output)
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        InitializeAsync().Wait();
    }

    private async Task InitializeAsync()
    {
        // Create main user
        var mainUser = new RegisterDto
        {
            DisplayName = "LikesMainUser",
            Email = "likesmain@test.com",
            Password = "Pa$$w0rd",
            Gender = "male",
            DateOfBirth = new DateOnly(1990, 1, 1),
            City = "TestCity",
            Country = "TestCountry"
        };

        var mainResponse = await _client.PostAsJsonAsync("/api/account/register", mainUser);
        var mainDto = await mainResponse.Content.ReadFromJsonAsync<UserDto>();

        _memberId = mainDto!.Id;
        _authToken = mainDto.Token;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

        // Create target users to like
        for (int i = 0; i < 30; i++)
        {
            var targetUser = new RegisterDto
            {
                DisplayName = $"LikeTarget{i}",
                Email = $"liketarget{i}@test.com",
                Password = "Pa$$w0rd",
                Gender = i % 2 == 0 ? "male" : "female",
                DateOfBirth = new DateOnly(1990 + i % 10, 1, 1),
                City = $"City{i}",
                Country = "TestCountry"
            };

            var targetResponse = await _client.PostAsJsonAsync("/api/account/register", targetUser);
            var targetDto = await targetResponse.Content.ReadFromJsonAsync<UserDto>();
            _targetMemberIds.Add(targetDto!.Id);
        }

        // Create some initial likes
        for (int i = 0; i < 5; i++)
        {
            await _client.PostAsync($"/api/likes/{_targetMemberIds[i]}", null);
        }
    }

    [Fact]
    public async Task ToggleLike_Add_Performance_Sequential()
    {
        // Arrange
        var iterations = 20;
        var counter = 5; // Start from 5 since we already liked 0-4

        // Act
        var result = await MeasurePerformanceAsync(
            "Add Like",
            async () =>
            {
                var index = Interlocked.Increment(ref counter);
                if (index < _targetMemberIds.Count)
                {
                    var response = await _client.PostAsync($"/api/likes/{_targetMemberIds[index]}", null);
                    response.EnsureSuccessStatusCode();
                }
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 200, maxP95Ms: 400);
    }

    [Fact]
    public async Task ToggleLike_Remove_Performance()
    {
        // Arrange - Create likes to remove
        var likesToRemove = new List<string>();
        for (int i = 0; i < 20; i++)
        {
            var targetId = _targetMemberIds[i % _targetMemberIds.Count];
            likesToRemove.Add(targetId);
            await _client.PostAsync($"/api/likes/{targetId}", null);
        }

        var iterations = 15;
        var counter = 0;

        // Act
        var result = await MeasurePerformanceAsync(
            "Remove Like",
            async () =>
            {
                var index = Interlocked.Increment(ref counter) - 1;
                if (index < likesToRemove.Count)
                {
                    var response = await _client.PostAsync($"/api/likes/{likesToRemove[index]}", null);
                    response.EnsureSuccessStatusCode();
                }
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 200, maxP95Ms: 400);
    }

    [Fact]
    public async Task GetCurrentMemberLikeIds_Performance()
    {
        // Arrange
        var iterations = 100;

        // Act
        var result = await MeasurePerformanceAsync(
            "Get Current Member Like IDs",
            async () =>
            {
                var response = await _client.GetAsync("/api/likes/list");
                response.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 150, maxP95Ms: 300);
    }

    [Fact]
    public async Task GetMemberLikes_Performance()
    {
        // Arrange
        var iterations = 100;

        // Act
        var result = await MeasurePerformanceAsync(
            "Get Member Likes",
            async () =>
            {
                var response = await _client.GetAsync("/api/likes?pageNumber=1&pageSize=10&predicate=liked");
                response.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 200, maxP95Ms: 400);
    }

    [Fact]
    public async Task GetMemberLikes_WithPagination_Performance()
    {
        // Arrange
        var iterations = 50;
        var pageCounter = 0;

        // Act
        var result = await MeasurePerformanceAsync(
            "Get Member Likes with Pagination",
            async () =>
            {
                var page = (Interlocked.Increment(ref pageCounter) % 3) + 1;
                var response = await _client.GetAsync($"/api/likes?pageNumber={page}&pageSize=5&predicate=liked");
                response.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 200, maxP95Ms: 400);
    }

    [Fact]
    public async Task GetMemberLikes_Performance_Concurrent()
    {
        // Act
        var result = await MeasureConcurrentPerformanceAsync(
            "Get Member Likes Concurrent",
            async () =>
            {
                var client = _factory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

                var response = await client.GetAsync("/api/likes?pageNumber=1&pageSize=10&predicate=liked");
                response.EnsureSuccessStatusCode();
            },
            concurrentRequests: 30,
            iterations: 3);

        // Assert
        AssertPerformance(result, maxAverageMs: 400, maxP95Ms: 800, minSuccessRate: 0.90);
    }

    [Fact]
    public async Task Likes_FullWorkflow_Performance()
    {
        // Arrange
        var iterations = 25;

        // Act
        var result = await MeasurePerformanceAsync(
            "Likes Full Workflow (Add->Get->Remove)",
            async () =>
            {
                var targetId = _targetMemberIds[Random.Shared.Next(_targetMemberIds.Count)];

                // Add like
                var addResponse = await _client.PostAsync($"/api/likes/{targetId}", null);
                addResponse.EnsureSuccessStatusCode();

                // Get likes list
                var getListResponse = await _client.GetAsync("/api/likes/list");
                getListResponse.EnsureSuccessStatusCode();

                // Get member likes
                var getMembersResponse = await _client.GetAsync("/api/likes?pageNumber=1&pageSize=10&predicate=liked");
                getMembersResponse.EnsureSuccessStatusCode();

                // Remove like
                var removeResponse = await _client.PostAsync($"/api/likes/{targetId}", null);
                removeResponse.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 600, maxP95Ms: 1000);
    }

    [Fact]
    public async Task ToggleLike_SelfLike_Performance()
    {
        // Arrange
        var iterations = 50;

        // Act
        var result = await MeasurePerformanceAsync(
            "Toggle Like Self (Error Case)",
            async () =>
            {
                var response = await _client.PostAsync($"/api/likes/{_memberId}", null);
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 100, maxP95Ms: 200);
    }

    [Fact]
    public async Task Likes_HighLoad_Performance()
    {
        // Act
        var result = await MeasureConcurrentPerformanceAsync(
            "Likes Under High Load",
            async () =>
            {
                var client = _factory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

                var operation = Random.Shared.Next(0, 4);

                if (operation == 0)
                {
                    // Toggle like
                    var targetId = _targetMemberIds[Random.Shared.Next(_targetMemberIds.Count)];
                    var response = await client.PostAsync($"/api/likes/{targetId}", null);
                    response.EnsureSuccessStatusCode();
                }
                else if (operation == 1)
                {
                    // Get like IDs
                    var response = await client.GetAsync("/api/likes/list");
                    response.EnsureSuccessStatusCode();
                }
                else
                {
                    // Get member likes
                    var page = Random.Shared.Next(1, 4);
                    var response = await client.GetAsync($"/api/likes?pageNumber={page}&pageSize=10&predicate=liked");
                    response.EnsureSuccessStatusCode();
                }
            },
            concurrentRequests: 40,
            iterations: 3);

        // Assert
        AssertPerformance(result, maxAverageMs: 600, maxP95Ms: 1200, minSuccessRate: 0.85);
    }

    [Fact]
    public async Task Likes_MassToggle_Performance()
    {
        // Arrange
        var counter = 0;

        // Act
        var result = await MeasureConcurrentPerformanceAsync(
            "Mass Like Toggle",
            async () =>
            {
                var client = _factory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

                var index = Interlocked.Increment(ref counter) % _targetMemberIds.Count;
                var targetId = _targetMemberIds[index];

                var response = await client.PostAsync($"/api/likes/{targetId}", null);
                response.EnsureSuccessStatusCode();
            },
            concurrentRequests: 30,
            iterations: 2);

        // Assert
        AssertPerformance(result, maxAverageMs: 500, maxP95Ms: 1000, minSuccessRate: 0.85);
    }

    public override void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
        base.Dispose();
    }
}