using System.Net;
using System.Net.Http.Json;
using Bogus;
using CoreCodeCamp.Data;
using CoreCodeCamp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreCodeCamp.Tests.Integration;

public class CreateCampIntegrationTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory = factory;
    private readonly Faker _faker = new();
    private HttpClient _client = null!;

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
    public async Task Post_WithValidCommand_ShouldCreateCampAndReturn201Async()
    {
        // Arrange
        var request = new CreateCampRequest
        {
            Name = _faker.Company.CompanyName() + " Code Camp",
            City = _faker.Random.AlphaNumeric(10).ToUpper(),
            EventDate = _faker.Date.Future(),
            Length = _faker.Random.Int(1, 5),
            LocationId = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/values", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CampContext>();
        var camp = await context.Camps.FirstOrDefaultAsync(c => c.City == request.City);

        camp.Should().NotBeNull();
        camp!.Name.Should().Be(request.Name);
        camp.EventDate.Should().Be(request.EventDate);
        camp.Length.Should().Be(request.Length);
    }

    [Fact]
    public async Task Post_ThenGet_ShouldReturnCreatedCampAsync()
    {
        // Arrange
        var request = new CreateCampRequest
        {
            Name = _faker.Company.CompanyName() + " Test Camp",
            City = _faker.Random.AlphaNumeric(10).ToUpper(),
            EventDate = _faker.Date.Future(),
            Length = _faker.Random.Int(1, 5),
            LocationId = 1
        };

        // Act - Create the camp
        var createResponse = await _client.PostAsJsonAsync("/api/values", request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Retrieve all camps
        var getResponse = await _client.GetAsync("/api/values");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var camps = await getResponse.Content.ReadFromJsonAsync<string[]>();

        // Assert
        camps.Should().NotBeNull();
        camps.Should().Contain(request.Name);
    }
}
