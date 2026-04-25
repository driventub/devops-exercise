using DevOpsService.Api.Middleware;
using DevOpsService.Api.Service;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace DevOpsService.UnitTests
{
    public class ApiKeyMiddlewareTests
    {
        private const string ValidApiKey = "2f5ae96c-b558-4c7b-a590-a501ae1c3f6c";

        private static ApiKeyMiddleware CreateMiddleware(RequestDelegate next)
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "ApiKey", ValidApiKey }
                })
                .Build();

            return new ApiKeyMiddleware(next, config);
        }

        private static DefaultHttpContext CreateContext(
            string method = "POST",
            string? apiKey = ValidApiKey,
            string? jwt = "some-jwt")
        {
            DefaultHttpContext context = new DefaultHttpContext();
            context.Request.Method = method;
            context.Response.Body = new MemoryStream();

            if (apiKey is not null)
            {
                context.Request.Headers["X-Parse-REST-API-Key"] = apiKey;
            }

            if (jwt is not null)
            {
                context.Request.Headers["X-JWT-KWY"] = jwt;
            }

            return context;
        }

        [Fact]
        public async Task InvokeAsyncValidRequestCallsNext()
        {
            // Arrange
            var nextCalled = false;
            var middleware = CreateMiddleware(_ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

            var context = CreateContext();
            JwtService jwtService = new JwtService();

            // Act
            await middleware.InvokeAsync(context, jwtService);

            // Assert
            _ = nextCalled.Should().BeTrue();
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("PATCH")]
        public async Task InvokeAsyncNonPostMethodReturns405AndErrorMessage(string method)
        {
            // Arrange
            var middleware = CreateMiddleware(_ => Task.CompletedTask);
            var context = CreateContext(method: method);
            JwtService jwtService = new JwtService();

            // Act
            await middleware.InvokeAsync(context, jwtService);

            // Assert
            _ = context.Response.StatusCode.Should().Be(405);
            _ = context.Response.Body.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
            _ = body.Should().Be("ERROR");
        }

        [Fact]
        public async Task InvokeAsyncMissingApiKeyReturns401()
        {
            // Arrange
            var middleware = CreateMiddleware(_ => Task.CompletedTask);
            var context = CreateContext(apiKey: null);
            JwtService jwtService = new JwtService();

            // Act
            await middleware.InvokeAsync(context, jwtService);

            // Assert
            _ = context.Response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task InvokeAsyncWrongApiKeyReturns401()
        {
            // Arrange
            var middleware = CreateMiddleware(_ => Task.CompletedTask);
            var context = CreateContext(apiKey: "wrong-key");
            JwtService jwtService = new JwtService();

            // Act
            await middleware.InvokeAsync(context, jwtService);

            // Assert
            _ = context.Response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task InvokeAsyncMissingJwtReturns401()
        {
            // Arrange
            var middleware = CreateMiddleware(_ => Task.CompletedTask);
            var context = CreateContext(jwt: null);
            JwtService jwtService = new JwtService();

            // Act
            await middleware.InvokeAsync(context, jwtService);

            // Assert
            _ = context.Response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task InvokeAsyncDuplicateJwtReturns401OnSecondCall()
        {
            // Arrange
            var middleware = CreateMiddleware(_ => Task.CompletedTask);
            JwtService jwtService = new JwtService();
            var jwt = "unique-jwt-token";

            var firstContext = CreateContext(jwt: jwt);
            var secondContext = CreateContext(jwt: jwt);

            // Act
            await middleware.InvokeAsync(firstContext, jwtService);
            await middleware.InvokeAsync(secondContext, jwtService);

            // Assert
            _ = secondContext.Response.StatusCode.Should().Be(401);
        }
    }
}