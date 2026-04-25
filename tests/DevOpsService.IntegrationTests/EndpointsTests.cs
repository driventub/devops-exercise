using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DevOpsService.IntegrationTests;

public class EndpointTest(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();
    private const string ApiKey = "test-api-key";

    private static StringContent BuildBody() =>
        new(JsonSerializer.Serialize(new
        {
            message = "This is a test",
            to = "Juan Perez",
            from = "Rita Asturia",
            timeToLifeSec = 45
        }), Encoding.UTF8, "application/json");

    private static string GenerateJwt() =>
        $"test-jwt-{Guid.NewGuid()}";


    [Fact]
    public async Task Post_WithValidRequest_Returns200AndExpectedMessage()
    {
        var config = factory.Services.GetRequiredService<IConfiguration>();
        var apiKey = config["ApiKey"];
        var request = new HttpRequestMessage(HttpMethod.Post, "/DevOps");
        request.Headers.Add("X-Parse-REST-API-Key", apiKey);
        request.Headers.Add("X-JWT-KWY", GenerateJwt());
        request.Content = BuildBody();

        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().Contain("Hello Juan Perez your message will be sent");
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public async Task NonPost_Returns405WithErrorMessage(string method)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), "/DevOps");
        request.Headers.Add("X-Parse-REST-API-Key", ApiKey);
        request.Headers.Add("X-JWT-KWY", GenerateJwt());

        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        body.Should().Be("ERROR");
    }


    [Fact]
    public async Task Post_WithoutApiKey_Returns401()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/DevOps");
        request.Headers.Add("X-JWT-KWY", GenerateJwt());
        request.Content = BuildBody();

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }


    [Fact]
    public async Task Post_WithWrongApiKey_Returns401()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/DevOps");
        request.Headers.Add("X-Parse-REST-API-Key", "wrong-key");
        request.Headers.Add("X-JWT-KWY", GenerateJwt());
        request.Content = BuildBody();

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }


    [Fact]
    public async Task Post_WithoutJwt_Returns401()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/DevOps");
        request.Headers.Add("X-Parse-REST-API-Key", ApiKey);
        request.Content = BuildBody();

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }


    [Fact]
    public async Task Post_WithDuplicateJwt_Returns401OnSecondCall()
    {
        var jwt = GenerateJwt();

        var first = new HttpRequestMessage(HttpMethod.Post, "/DevOps");
        first.Headers.Add("X-Parse-REST-API-Key", ApiKey);
        first.Headers.Add("X-JWT-KWY", jwt);
        first.Content = BuildBody();

        var second = new HttpRequestMessage(HttpMethod.Post, "/DevOps");
        second.Headers.Add("X-Parse-REST-API-Key", ApiKey);
        second.Headers.Add("X-JWT-KWY", jwt);
        second.Content = BuildBody();

        await _client.SendAsync(first);
        var response = await _client.SendAsync(second);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}