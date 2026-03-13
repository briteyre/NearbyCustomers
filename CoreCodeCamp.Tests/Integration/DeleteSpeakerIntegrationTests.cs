using System.Net;
using System.Net.Http.Json;
using Bogus;
using CoreCodeCamp.Data;
using CoreCodeCamp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreCodeCamp.Tests.Integration;

public class DeleteSpeakerIntegrationTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private readonly Faker _faker = new();

    public DeleteSpeakerIntegrationTests(TestWebApplicationFactory factory)
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
    public async Task Delete_WithExistingSpeaker_ShouldRemoveSpeakerAndReturn200Async()
    {
        // Arrange - create a speaker
        var request = new CreateSpeakerRequest
        {
            FirstName = _faker.Name.FirstName(),
            LastName = _faker.Name.LastName(),
            MiddleName = _faker.Random.String2(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ"),
            Company = _faker.Company.CompanyName(),
            Twitter = $"@{_faker.Internet.UserName()}",
            GitHub = _faker.Internet.UserName()
        };

        var postResponse = await _client.PostAsJsonAsync("/api/speakers", request);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CampContext>();
            var speaker = await context.Speakers.FirstOrDefaultAsync(s => s.FirstName == request.FirstName && s.LastName == request.LastName);
            speaker.Should().NotBeNull();
        }

        // Act - delete
        var deleteResponse = await _client.DeleteAsync($"/api/speakers/{Uri.EscapeDataString(request.FirstName)}/{Uri.EscapeDataString(request.LastName)}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CampContext>();
            var speaker = await context.Speakers.FirstOrDefaultAsync(s => s.FirstName == request.FirstName && s.LastName == request.LastName);
            speaker.Should().BeNull();
        }
    }

    [Fact]
    public async Task Delete_NonExistentSpeaker_ShouldReturn404Async()
    {
        // Arrange - random names unlikely to exist
        var first = _faker.Name.FirstName();
        var last = _faker.Name.LastName();

        // Act
        var response = await _client.DeleteAsync($"/api/speakers/{Uri.EscapeDataString(first)}/{Uri.EscapeDataString(last)}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
