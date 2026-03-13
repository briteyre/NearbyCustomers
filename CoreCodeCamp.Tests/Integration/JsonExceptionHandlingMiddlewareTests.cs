using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CoreCodeCamp.Tests.Integration;

public class JsonExceptionHandlingMiddlewareTests : IAsyncLifetime
{
    private HttpClient _httpClient = null!;
    private WebApplicationFactory<Program> _factory = null!;

    public async Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>();
        _httpClient = _factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _httpClient.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task CreateCamp_WithInvalidEventDate_ReturnsJsonError()
    {
        // Arrange
        var invalidPayload = new
        {
            name = "Test Camp",
            city = "Test City",
            eventDate = "", // Invalid empty string
            length = 1
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/values", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be(MediaTypes.ProblemJson);

        var responseBody = JsonNode.Parse(await response.Content.ReadAsStringAsync());

        // Verify response structure
        responseBody.Should().NotBeNull();
        responseBody!["title"]!.GetValue<string>().Should().Be("Invalid JSON format");
        responseBody!["status"]!.GetValue<int>().Should().Be(400);
        responseBody!["errors"].Should().NotBeNull();

        // Verify the property name is extracted correctly
        var errors = responseBody!["errors"]!.AsObject();
        errors.Should().ContainKey("eventDate");
    }

    [Fact]
    public async Task CreateCamp_WithNullEventDate_ReturnsClearJsonError()
    {
        // Arrange - explicitly send null for eventDate
        var invalidPayload = new
        {
            name = "Test Camp",
            city = "Test City",
            eventDate = (string?)null,
            length = 1
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/values", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be(MediaTypes.ProblemJson);

        var responseBody = JsonNode.Parse(await response.Content.ReadAsStringAsync());
        responseBody!["title"]!.GetValue<string>().Should().Be("Invalid JSON format");

        var errors = responseBody!["errors"]!.AsObject();
        errors.Should().ContainKey("eventDate");
    }

    [Fact]
    public async Task CreateCamp_WithValidData_DoesNotTriggerJsonException()
    {
        // Arrange
        var validPayload = new
        {
            name = "Valid Camp",
            city = "Valid City",
            eventDate = "2025-12-25",
            length = 2
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/values", validPayload);

        // Assert 
        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task JsonError_ResponseIncludesDetailMessage()
    {
        // Arrange
        var invalidPayload = new
        {
            name = "Test",
            city = "City",
            eventDate = "not-a-date",
            length = 1
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/values", invalidPayload);

        // Assert
        var responseBody = JsonNode.Parse(await response.Content.ReadAsStringAsync());
        responseBody!["detail"]!.GetValue<string>().Should().Contain("Failed to parse JSON");
    }
}
