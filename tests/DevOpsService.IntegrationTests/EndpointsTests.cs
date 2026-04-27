using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DevOpsService.IntegrationTests
{
    public class EndpointTest(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client = factory.CreateClient();
        private const string _apiKey = "test-api-key";

        private static StringContent BuildBody()
        {
            return new(JsonSerializer.Serialize(new
            {
                message = "This is a test",
                to = "Juan Perez",
                from = "Rita Asturia",
                timeToLifeSec = 45
            }), Encoding.UTF8, "application/json");
        }

        private static string GenerateJwt()
        {
            return $"test-jwt-{Guid.NewGuid()}";
        }

        [Fact]
        public async Task PostWithValidRequestReturns200AndExpectedMessage()
        {
            IConfiguration config = factory.Services.GetRequiredService<IConfiguration>();
            string? apiKey = config["ApiKey"];
            HttpRequestMessage request = new(HttpMethod.Post, "/DevOps");
            request.Headers.Add("X-Parse-REST-API-Key", apiKey);
            request.Headers.Add("X-JWT-KWY", GenerateJwt());
            request.Content = BuildBody();

            HttpResponseMessage response = await _client.SendAsync(request);
            string body = await response.Content.ReadAsStringAsync();

            _ = response.StatusCode.Should().Be(HttpStatusCode.OK);
            _ = body.Should().Contain("Hello Juan Perez your message will be sent");
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("PATCH")]
        public async Task NonPostReturns405WithErrorMessage(string method)
        {
            HttpRequestMessage request = new(new HttpMethod(method), "/DevOps");
            request.Headers.Add("X-Parse-REST-API-Key", _apiKey);
            request.Headers.Add("X-JWT-KWY", GenerateJwt());

            HttpResponseMessage response = await _client.SendAsync(request);
            string body = await response.Content.ReadAsStringAsync();

            _ = body.Should().Be("ERROR");
        }


        [Fact]
        public async Task PostWithoutApiKeyReturns401()
        {
            HttpRequestMessage request = new(HttpMethod.Post, "/DevOps");
            request.Headers.Add("X-JWT-KWY", GenerateJwt());
            request.Content = BuildBody();

            HttpResponseMessage response = await _client.SendAsync(request);

            _ = response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }


        [Fact]
        public async Task PostWithWrongApiKeyReturns401()
        {
            HttpRequestMessage request = new(HttpMethod.Post, "/DevOps");
            request.Headers.Add("X-Parse-REST-API-Key", "wrong-key");
            request.Headers.Add("X-JWT-KWY", GenerateJwt());
            request.Content = BuildBody();

            HttpResponseMessage response = await _client.SendAsync(request);

            _ = response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }


        [Fact]
        public async Task PostWithoutJwtReturns401()
        {
            HttpRequestMessage request = new(HttpMethod.Post, "/DevOps");
            request.Headers.Add("X-Parse-REST-API-Key", _apiKey);
            request.Content = BuildBody();

            HttpResponseMessage response = await _client.SendAsync(request);

            _ = response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }


        [Fact]
        public async Task PostWithDuplicateJwtReturns401OnSecondCall()
        {
            string jwt = GenerateJwt();

            HttpRequestMessage first = new(HttpMethod.Post, "/DevOps");
            first.Headers.Add("X-Parse-REST-API-Key", _apiKey);
            first.Headers.Add("X-JWT-KWY", jwt);
            first.Content = BuildBody();

            HttpRequestMessage second = new(HttpMethod.Post, "/DevOps");
            second.Headers.Add("X-Parse-REST-API-Key", _apiKey);
            second.Headers.Add("X-JWT-KWY", jwt);
            second.Content = BuildBody();

            _ = await _client.SendAsync(first);
            HttpResponseMessage response = await _client.SendAsync(second);

            _ = response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}