using API.Errors;
using API.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;

namespace Api.Tests.Middleware
{
    public class ExceptionMiddlewareTests
    {
        private static HttpContext CreateHttpContext()
        {
            return new DefaultHttpContext();
        }

        private static RequestDelegate ThrowingDelegate()
        {
            return _ => throw new Exception("Test exception");
        }

        private static RequestDelegate SuccessDelegate()
        {
            return ctx =>
            {
                ctx.Response.StatusCode = 200;
                return Task.CompletedTask;
            };
        }

        [Fact]
        public async Task Should_Return500_And_LogError_InDevelopment()
        {
            var loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
            var envMock = new Mock<IHostEnvironment>();

            var middleware = new ExceptionMiddleware(
                ThrowingDelegate(),
                loggerMock.Object,
                envMock.Object
            );

            var context = CreateHttpContext();

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            context.Response.ContentType.Should().Be("application/json");

            context.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);
            var json = await new System.IO.StreamReader(context.Response.Body).ReadToEndAsync();

            var result = JsonSerializer.Deserialize<ApiException>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(500);
            result.Message.Should().Be("Test exception");
            result.Details.Should().NotBeNullOrEmpty();

            loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), "{message}", "Test exception"),
                Times.Once
            );
        }

        [Fact]
        public async Task Should_Return500_WithoutStackTrace_InProduction()
        {
            var loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
            var envMock = new Mock<IHostEnvironment>();

            envMock.SetupGet(x => x.EnvironmentName).Returns("Production");

            var middleware = new ExceptionMiddleware(
                ThrowingDelegate(),
                loggerMock.Object,
                envMock.Object
            );

            var context = CreateHttpContext();

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);

            context.Response.Body.Seek(0, System.IO.SeekOrigin.Begin);
            var json = await new System.IO.StreamReader(context.Response.Body).ReadToEndAsync();

            var result = JsonSerializer.Deserialize<ApiException>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            result!.Message.Should().Be("Test exception");
            result.Details.Should().Be("Internal server error"); // expected fallback
        }

        [Fact]
        public async Task Should_Not_Alter_Response_When_No_Exception()
        {
            var loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
            var envMock = new Mock<IHostEnvironment>();

            var middleware = new ExceptionMiddleware(
                SuccessDelegate(),
                loggerMock.Object,
                envMock.Object
            );

            var context = CreateHttpContext();

            await middleware.InvokeAsync(context);

            context.Response.StatusCode.Should().Be(200);

            loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Never
            );
        }
    }
}