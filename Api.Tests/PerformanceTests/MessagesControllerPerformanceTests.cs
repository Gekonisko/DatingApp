using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Api.Tests.FunctionalTests;
using API.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace Api.Tests.PerformanceTests;

[Collection("Performance")]
public class MessagesControllerPerformanceTests : PerformanceTestBase
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private string _authToken = string.Empty;
    private string _senderId = string.Empty;
    private string _recipientId = string.Empty;

    public MessagesControllerPerformanceTests(ITestOutputHelper output) : base(output)
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
        InitializeAsync().Wait();
    }

    private async Task InitializeAsync()
    {
        // Create sender
        var senderRegister = new RegisterDto
        {
            DisplayName = "MessageSender",
            Email = "sender@test.com",
            Password = "Pa$$w0rd",
            Gender = "male",
            DateOfBirth = new DateOnly(1990, 1, 1),
            City = "TestCity",
            Country = "TestCountry"
        };

        var senderResponse = await _client.PostAsJsonAsync("/api/account/register", senderRegister);
        var senderDto = await senderResponse.Content.ReadFromJsonAsync<UserDto>();
        
        _senderId = senderDto!.Id;
        _authToken = senderDto.Token;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

        // Create recipient
        var recipientRegister = new RegisterDto
        {
            DisplayName = "MessageRecipient",
            Email = "recipient@test.com",
            Password = "Pa$$w0rd",
            Gender = "female",
            DateOfBirth = new DateOnly(1992, 1, 1),
            City = "TestCity",
            Country = "TestCountry"
        };

        var recipientResponse = await _client.PostAsJsonAsync("/api/account/register", recipientRegister);
        var recipientDto = await recipientResponse.Content.ReadFromJsonAsync<UserDto>();
        _recipientId = recipientDto!.Id;

        // Create some initial messages
        for (int i = 0; i < 10; i++)
        {
            var message = new CreateMessageDto
            {
                RecipientId = _recipientId,
                Content = $"Test message {i}"
            };
            await _client.PostAsJsonAsync("/api/messages", message);
        }
    }

    [Fact]
    public async Task CreateMessage_Performance_Sequential()
    {
        // Arrange
        var iterations = 50;
        var counter = 100;

        // Act
        var result = await MeasurePerformanceAsync(
            "Create Message",
            async () =>
            {
                var message = new CreateMessageDto
                {
                    RecipientId = _recipientId,
                    Content = $"Performance test message {Interlocked.Increment(ref counter)}"
                };

                var response = await _client.PostAsJsonAsync("/api/messages", message);
                response.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 300, maxP95Ms: 600);
    }

    [Fact]
    public async Task CreateMessage_Performance_Concurrent()
    {
        // Arrange
        var counter = 1000;

        // Act
        var result = await MeasureConcurrentPerformanceAsync(
            "Create Message Concurrent",
            async () =>
            {
                var client = _factory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                
                var message = new CreateMessageDto
                {
                    RecipientId = _recipientId,
                    Content = $"Concurrent message {Interlocked.Increment(ref counter)}"
                };

                var response = await client.PostAsJsonAsync("/api/messages", message);
                response.EnsureSuccessStatusCode();
            },
            concurrentRequests: 20,
            iterations: 5);

        // Assert
        AssertPerformance(result, maxAverageMs: 500, maxP95Ms: 1000, minSuccessRate: 0.90);
    }

    [Fact]
    public async Task GetMessages_Performance_Sequential()
    {
        // Arrange
        var iterations = 100;

        // Act
        var result = await MeasurePerformanceAsync(
            "Get Messages",
            async () =>
            {
                var response = await _client.GetAsync("/api/messages?container=Inbox&pageNumber=1&pageSize=10");
                response.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 200, maxP95Ms: 400);
    }

    [Fact]
    public async Task GetMessages_Inbox_Performance()
    {
        // Arrange
        var iterations = 50;

        // Act
        var result = await MeasurePerformanceAsync(
            "Get Inbox Messages",
            async () =>
            {
                var response = await _client.GetAsync("/api/messages?container=Inbox&pageNumber=1&pageSize=20");
                response.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 200, maxP95Ms: 400);
    }

    [Fact]
    public async Task GetMessages_Outbox_Performance()
    {
        // Arrange
        var iterations = 50;

        // Act
        var result = await MeasurePerformanceAsync(
            "Get Outbox Messages",
            async () =>
            {
                var response = await _client.GetAsync("/api/messages?container=Outbox&pageNumber=1&pageSize=20");
                response.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 200, maxP95Ms: 400);
    }

    [Fact]
    public async Task GetMessageThread_Performance()
    {
        // Arrange
        var iterations = 100;

        // Act
        var result = await MeasurePerformanceAsync(
            "Get Message Thread",
            async () =>
            {
                var response = await _client.GetAsync($"/api/messages/thread/{_recipientId}");
                response.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 200, maxP95Ms: 400);
    }

    [Fact]
    public async Task GetMessageThread_Performance_Concurrent()
    {
        // Act
        var result = await MeasureConcurrentPerformanceAsync(
            "Get Message Thread Concurrent",
            async () =>
            {
                var client = _factory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                
                var response = await client.GetAsync($"/api/messages/thread/{_recipientId}");
                response.EnsureSuccessStatusCode();
            },
            concurrentRequests: 30,
            iterations: 3);

        // Assert
        AssertPerformance(result, maxAverageMs: 400, maxP95Ms: 800, minSuccessRate: 0.90);
    }

    [Fact]
    public async Task DeleteMessage_Performance()
    {
        // Arrange - Create messages to delete
        var messageIds = new List<string>();
        for (int i = 0; i < 30; i++)
        {
            var message = new CreateMessageDto
            {
                RecipientId = _recipientId,
                Content = $"Message to delete {i}"
            };

            var response = await _client.PostAsJsonAsync("/api/messages", message);
            var messageDto = await response.Content.ReadFromJsonAsync<MessageDto>();
            messageIds.Add(messageDto!.Id);
        }

        var counter = 0;
        var iterations = 25;

        // Act
        var result = await MeasurePerformanceAsync(
            "Delete Message",
            async () =>
            {
                var index = Interlocked.Increment(ref counter) - 1;
                if (index < messageIds.Count)
                {
                    var response = await _client.DeleteAsync($"/api/messages/{messageIds[index]}");
                    response.EnsureSuccessStatusCode();
                }
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 300, maxP95Ms: 600);
    }

    [Fact]
    public async Task Messages_FullWorkflow_Performance()
    {
        // Arrange
        var iterations = 20;
        var counter = 5000;

        // Act
        var result = await MeasurePerformanceAsync(
            "Messages Full Workflow (Create->Get->Delete)",
            async () =>
            {
                var id = Interlocked.Increment(ref counter);
                
                // Create message
                var createDto = new CreateMessageDto
                {
                    RecipientId = _recipientId,
                    Content = $"Workflow test {id}"
                };

                var createResponse = await _client.PostAsJsonAsync("/api/messages", createDto);
                createResponse.EnsureSuccessStatusCode();
                var messageDto = await createResponse.Content.ReadFromJsonAsync<MessageDto>();

                // Get thread
                var getResponse = await _client.GetAsync($"/api/messages/thread/{_recipientId}");
                getResponse.EnsureSuccessStatusCode();

                // Delete message
                var deleteResponse = await _client.DeleteAsync($"/api/messages/{messageDto!.Id}");
                deleteResponse.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 600, maxP95Ms: 1000);
    }

    [Fact]
    public async Task GetMessages_WithPagination_Performance()
    {
        // Arrange
        var iterations = 40;
        var pageCounter = 0;

        // Act
        var result = await MeasurePerformanceAsync(
            "Get Messages with Pagination",
            async () =>
            {
                var page = (Interlocked.Increment(ref pageCounter) % 3) + 1;
                var response = await _client.GetAsync($"/api/messages?container=Outbox&pageNumber={page}&pageSize=5");
                response.EnsureSuccessStatusCode();
            },
            iterations);

        // Assert
        AssertPerformance(result, maxAverageMs: 200, maxP95Ms: 400);
    }

    [Fact]
    public async Task Messages_HighLoad_Performance()
    {
        // Arrange
        var counter = 10000;

        // Act
        var result = await MeasureConcurrentPerformanceAsync(
            "Messages Under High Load",
            async () =>
            {
                var client = _factory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                
                var operation = Random.Shared.Next(0, 3);
                
                if (operation == 0)
                {
                    // Create message
                    var message = new CreateMessageDto
                    {
                        RecipientId = _recipientId,
                        Content = $"High load message {Interlocked.Increment(ref counter)}"
                    };
                    var response = await client.PostAsJsonAsync("/api/messages", message);
                    response.EnsureSuccessStatusCode();
                }
                else if (operation == 1)
                {
                    // Get inbox
                    var response = await client.GetAsync("/api/messages?container=Inbox&pageNumber=1&pageSize=10");
                    response.EnsureSuccessStatusCode();
                }
                else
                {
                    // Get thread
                    var response = await client.GetAsync($"/api/messages/thread/{_recipientId}");
                    response.EnsureSuccessStatusCode();
                }
            },
            concurrentRequests: 40,
            iterations: 3);

        // Assert
        AssertPerformance(result, maxAverageMs: 700, maxP95Ms: 1400, minSuccessRate: 0.85);
    }

    public override void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
        base.Dispose();
    }
}
