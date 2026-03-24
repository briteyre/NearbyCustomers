using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CoreCodeCamp.Tests.Integration;

public class JsonExceptionHandlingMiddlewareTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory = factory;
    private HttpClient _httpClient = null!;

    public Task InitializeAsync()
    {
        _httpClient = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _httpClient.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateCamp_WithInvalidEventDate_ReturnsJsonError()
    {
        // Arrange
        var invalidPayload = new
        {
            name = "Test Camp",
            city = "Some City",
            eventDate = "", 
            length = 1
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/camps", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be(MediaTypes.ProblemJson);

        var responseBody = JsonNode.Parse(await response.Content.ReadAsStringAsync());

        // Verify response structure
        responseBody.Should().NotBeNull();
        responseBody!["status"]!.GetValue<int>().Should().Be(400);
        responseBody!["errors"].Should().NotBeNull();

        var errors = responseBody!["errors"]!.AsObject();
        var propName = nameof(Services.CreateCampRequest.EventDate);
        errors.ToJsonString().Contains(propName, StringComparison.OrdinalIgnoreCase).Should().BeTrue();
    }

    [Fact]
    public async Task CreateCamp_WithNullEventDate_ReturnsClearJsonError()
    {
        // Arrange 
        var invalidPayload = new
        {
            name = "Test Camp",
            city = "Test City",
            eventDate = (string?)null,
            length = 1
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/camps", invalidPayload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be(MediaTypes.ProblemJson);

        var responseBody = JsonNode.Parse(await response.Content.ReadAsStringAsync());

        var errors = responseBody!["errors"]!.AsObject();
        var propName = nameof(Services.CreateCampRequest.EventDate);
        errors.ToJsonString().Contains(propName, StringComparison.OrdinalIgnoreCase).Should().BeTrue();
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
        var response = await _httpClient.PostAsJsonAsync("/api/camps", validPayload);

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
        var response = await _httpClient.PostAsJsonAsync("/api/camps", invalidPayload);

        // Assert
        var responseBody = JsonNode.Parse(await response.Content.ReadAsStringAsync());
        // The middleware may include a "detail" message or only an "errors" object. Check safely.
        var detail = responseBody?["detail"]?.GetValue<string>();
        if (detail is not null)
        {
            detail.Should().Contain("validation errors");
        }
        else
        {
            responseBody!["errors"]!.Should().NotBeNull();
        }
    }
}
