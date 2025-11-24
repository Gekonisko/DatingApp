using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;

namespace Api.Tests.Controllers
{
    public class BuggyControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public BuggyControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Auth_ShouldReturn401()
        {
            var response = await _client.GetAsync("/api/buggy/auth");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}