using System.Net;
using System.Net.Http.Json;
using Bogus;
using CoreCodeCamp.Data;
using CoreCodeCamp.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreCodeCamp.Tests.Integration;

public class UpdateCampIntegrationTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    private readonly Faker _faker = new();
    private HttpClient _client = null!;

    public UpdateCampIntegrationTests(TestWebApplicationFactory factory)
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
    public async Task Put_WithValidRequest_ShouldUpdateCampAndReturn200Async()
    {
        // Arrange - Create initial camp
        var createRequest = new CreateCampRequest
        {
            Name = _faker.Company.CompanyName() + " Original Camp",
            City = _faker.Random.AlphaNumeric(10).ToUpper(),
            EventDate = _faker.Date.Future(),
            Length = _faker.Random.Int(1, 5),
            LocationId = 1
        };
        await _client.PostAsJsonAsync("/api/values", createRequest);

        var updateRequest = new UpdateCampRequest
        {
            Name = _faker.Company.CompanyName() + " Updated Camp",
            EventDate = _faker.Date.Future(),
            Length = _faker.Random.Int(1, 5)
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/values/{createRequest.City}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CampContext>();
        var camp = await context.Camps.FirstOrDefaultAsync(c => c.City == createRequest.City);

        camp.Should().NotBeNull();
        camp!.Name.Should().Be(updateRequest.Name);
        camp.EventDate.Should().Be(updateRequest.EventDate);
        camp.Length.Should().Be(updateRequest.Length);
    }

    [Fact]
    public async Task Put_WithNonExistentCity_ShouldReturn404Async()
    {
        // Arrange
        var updateRequest = new UpdateCampRequest
        {
            Name = _faker.Company.CompanyName() + " Updated Camp",
            EventDate = _faker.Date.Future(),
            Length = _faker.Random.Int(1, 5)
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/values/NONEXISTENT", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
