using System.Net;
using System.Net.Http.Json;
using Bogus;
using CoreCodeCamp.Data;
using CoreCodeCamp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreCodeCamp.Tests.Integration;

public class CreateSpeakerIntegrationTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private readonly Faker _faker = new();

    public CreateSpeakerIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CampContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync()
    {
        _client?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Post_WithValidCommand_ShouldCreateSpeakerAndReturn201Async()
    {
        // Arrange
        var request = new CreateSpeakerRequest
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            MiddleName = _faker.Random.String2(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ"),
            Company = _faker.Company.CompanyName(),
            CompanyUrl = _faker.Internet.Url(),
            BlogUrl = _faker.Internet.Url(),
            Twitter = $"@{_faker.Internet.UserName()}",
            GitHub = _faker.Internet.UserName()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/speakers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CampContext>();
        var speaker = await context.Speakers.FirstOrDefaultAsync(s => s.FirstName == request.FirstName && s.LastName == request.LastName);

        speaker.Should().NotBeNull();
        speaker!.FirstName.Should().Be(request.FirstName);
        speaker.LastName.Should().Be(request.LastName);
        speaker.MiddleName.Should().Be(request.MiddleName);
        speaker.Company.Should().Be(request.Company);
        speaker.CompanyUrl.Should().Be(request.CompanyUrl);
        speaker.BlogUrl.Should().Be(request.BlogUrl);
        speaker.Twitter.Should().Be(request.Twitter);
        speaker.GitHub.Should().Be(request.GitHub);
    }

    [Fact]
    public async Task Post_ShouldReturnSuccessResponseAsync()
    {
        // Arrange
        var request = new CreateSpeakerRequest
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            MiddleName = _faker.Random.String2(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ"),
            Company = _faker.Company.CompanyName(),
            Twitter = $"@{_faker.Internet.UserName()}",
            GitHub = _faker.Internet.UserName()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/speakers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("success");
    }

    [Fact]
    public async Task Put_WithValidRequest_ShouldUpdateSpeakerAndReturn200Async()
    {
        // Arrange - create speaker
        var createRequest = new CreateSpeakerRequest
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            MiddleName = _faker.Random.String2(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ"),
            Company = _faker.Company.CompanyName(),
            CompanyUrl = _faker.Internet.Url(),
            BlogUrl = _faker.Internet.Url(),
            Twitter = $"@{_faker.Internet.UserName()}",
            GitHub = _faker.Internet.UserName()
        };

        var postResponse = await _client.PostAsJsonAsync("/api/speakers", createRequest);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        int speakerId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CampContext>();
            var speaker = await context.Speakers.FirstOrDefaultAsync(s => s.FirstName == createRequest.FirstName && s.LastName == createRequest.LastName);
            speaker.Should().NotBeNull();
            speakerId = speaker!.SpeakerId;
        }

        var updateRequest = new UpdateSpeakerRequest
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            MiddleName = _faker.Random.String2(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ"),
            Company = _faker.Company.CompanyName(),
            CompanyUrl = _faker.Internet.Url(),
            BlogUrl = _faker.Internet.Url(),
            Twitter = $"@{_faker.Internet.UserName()}",
            GitHub = _faker.Internet.UserName()
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/speakers/{speakerId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CampContext>();
            var speaker = await context.Speakers.FirstOrDefaultAsync(s => s.SpeakerId == speakerId);

            speaker.Should().NotBeNull();
            speaker!.FirstName.Should().Be(updateRequest.FirstName);
            speaker.LastName.Should().Be(updateRequest.LastName);
            speaker.MiddleName.Should().Be(updateRequest.MiddleName);
            speaker.Company.Should().Be(updateRequest.Company);
            speaker.CompanyUrl.Should().Be(updateRequest.CompanyUrl);
            speaker.BlogUrl.Should().Be(updateRequest.BlogUrl);
            speaker.Twitter.Should().Be(updateRequest.Twitter);
            speaker.GitHub.Should().Be(updateRequest.GitHub);
        }
    }

    [Fact]
    public async Task Put_WithNonExistentSpeaker_ShouldReturn404Async()
    {
        // Arrange
        var updateRequest = new UpdateSpeakerRequest
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName()
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/speakers/99999", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
