using System.Net;
using System.Net.Http.Json;
using Bogus;
using CoreCodeCamp.Data;
using CoreCodeCamp.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreCodeCamp.Tests.Integration;

public class GetCampsIntegrationTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly Faker _faker = new();
    private HttpClient _client = null!;

    public GetCampsIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CampContext>();

        // Clear existing data instead of recreating schema
        context.Camps.RemoveRange(context.Camps);
        context.Speakers.RemoveRange(context.Speakers);
        context.Talks.RemoveRange(context.Talks);
        context.Locations.RemoveRange(context.Locations);
        await context.SaveChangesAsync();

        // Seed a location for testing
        context.Locations.Add(new Location
        {
            LocationId = 1,
            VenueName = "Denver Convention Center",
            Address1 = "700 14th St",
            Address2 = "",
            Address3 = "",
            CityTown = "Denver",
            StateProvince = "CO",
            PostalCode = "80202",
            Country = "USA"
        });
        await context.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        _client?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Get_WithNoCamps_ShouldReturnEmptyArrayAsync()
    {
        // Act
        var response = await _client.GetAsync("/api/values");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var camps = await response.Content.ReadFromJsonAsync<string[]>();
        camps.Should().NotBeNull();
        camps.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_WithMultipleCamps_ShouldReturnAllCampNamesAsync()
    {
        // Arrange
        var firstCampRequest = new CreateCampRequest
        {
            Name = _faker.Company.CompanyName() + " Camp",
            City = _faker.Random.AlphaNumeric(10).ToUpper(),
            EventDate = _faker.Date.Future(),
            Length = _faker.Random.Int(1, 5),
            LocationId = 1
        };

        var secondCampRequest = new CreateCampRequest
        {
            Name = _faker.Company.CompanyName() + " Camp",
            City = _faker.Random.AlphaNumeric(10).ToUpper(),
            EventDate = _faker.Date.Future(),
            Length = _faker.Random.Int(1, 5),
            LocationId = 1
        };

        await _client.PostAsJsonAsync("/api/values", firstCampRequest);
        await _client.PostAsJsonAsync("/api/values", secondCampRequest);

        // Act
        var response = await _client.GetAsync("/api/values");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var camps = await response.Content.ReadFromJsonAsync<string[]>();
        camps.Should().NotBeNull();
        camps.Should().HaveCount(2);
        camps.Should().Contain(firstCampRequest.Name);
        camps.Should().Contain(secondCampRequest.Name);
    }
}
