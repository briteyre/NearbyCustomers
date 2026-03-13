using CoreCodeCamp.Data;
using CoreCodeCamp.Services;
using Microsoft.Extensions.Logging;
using Bogus;

namespace CoreCodeCamp.Tests.Unit;

public class CampServiceTests
{
    private readonly Mock<ILogger<CampService>> _mockLogger = new();
    private const string NonExistent = "NONEXISTENT";

    private CampService CreateService(Mock<ICampRepository> mockRepo)
    {
        return new CampService(mockRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateSpeaker_ShouldCreateAndReturnSpeakerAsync()
    {
        // Arrange
        var mockRepo = new Mock<ICampRepository>();
        mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var service = CreateService(mockRepo);

        var faker = new Faker();
        var request = new CreateSpeakerRequest
        {
            FirstName = faker.Name.FirstName(),
            LastName = faker.Name.LastName(),
            MiddleName = faker.Random.String2(1, "DMY"),
            Company = faker.Company.CompanyName(),
            CompanyUrl = faker.Internet.Url(),
            BlogUrl = faker.Internet.Url(),
            Twitter = $"@{faker.Internet.UserName()}",
            GitHub = faker.Internet.UserName()
        };

        // Act
        var result = await service.CreateSpeakerAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
        result.MiddleName.Should().Be(request.MiddleName);
        result.Company.Should().Be(request.Company);
        result.CompanyUrl.Should().Be(request.CompanyUrl);
        result.BlogUrl.Should().Be(request.BlogUrl);
        result.Twitter.Should().Be(request.Twitter);
        result.GitHub.Should().Be(request.GitHub);

        mockRepo.Verify(r => r.Add(It.IsAny<Speaker>()), Times.Once);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateCamp_ShouldCreateAndReturnCampAsync()
    {
        // Arrange
        var location = new Location
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
        };
        var mockRepo = new Mock<ICampRepository>();
        mockRepo.Setup(r => r.GetLocationByIdAsync(1)).ReturnsAsync(location);
        mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var service = CreateService(mockRepo);

        var request = new CreateCampRequest
        {
            Name = "Test Camp",
            City = "TEST2024",
            EventDate = new DateTime(2024, 12, 1),
            Length = 3,
            LocationId = 1
        };

        // Act
        var result = await service.CreateCampAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.City.Should().Be(request.City);
        result.EventDate.Should().Be(request.EventDate);
        result.Length.Should().Be(request.Length);

        mockRepo.Verify(r => r.Add(It.IsAny<Camp>()), Times.Once);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateCamp_WithExistingCamp_ShouldUpdateAndReturnTrueAsync()
    {
        // Arrange
        var existingCamp = new Camp
        {
            CampId = 1,
            Name = "Old Name",
            City = "TEST2024",
            EventDate = new DateTime(2024, 1, 1),
            Length = 1
        };

        var mockRepo = new Mock<ICampRepository>();
        mockRepo.Setup(r => r.GetCampAsync("TEST2024", false)).ReturnsAsync(existingCamp);
        mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var service = CreateService(mockRepo);

        var request = new UpdateCampRequest
        {
            Name = "New Name",
            EventDate = new DateTime(2024, 12, 1),
            Length = 3
        };

        // Act
        var result = await service.UpdateCampAsync("TEST2024", request);

        // Assert
        result.Should().BeTrue();
        existingCamp.Name.Should().Be(request.Name);
        existingCamp.EventDate.Should().Be(request.EventDate);
        existingCamp.Length.Should().Be(request.Length);

        mockRepo.Verify(r => r.GetCampAsync(existingCamp.City, false), Times.Once);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateCamp_WithNonExistentCamp_ShouldReturnFalseAsync()
    {
        // Arrange
        var mockRepo = new Mock<ICampRepository>();
        mockRepo.Setup(r => r.GetCampAsync(NonExistent, false)).ReturnsAsync((Camp?)null);

        var service = CreateService(mockRepo);

        var request = new UpdateCampRequest
        {
            Name = "New Name",
            EventDate = new DateTime(2024, 12, 1),
            Length = 3
        };

        // Act
        var result = await service.UpdateCampAsync(NonExistent, request);

        // Assert
        result.Should().BeFalse();

        mockRepo.Verify(r => r.GetCampAsync(NonExistent, false), Times.Once);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAllCamps_ShouldReturnAllCampsAsync()
    {
        // Arrange
        var camps = new[]
        {
            new Camp { CampId = 1, Name = "Camp 1", City = "C1" },
            new Camp { CampId = 2, Name = "Camp 2", City = "C2" }
        };

        var mockRepo = new Mock<ICampRepository>();
        mockRepo.Setup(r => r.GetAllCampsAsync(false)).ReturnsAsync(camps);

        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetAllCampsAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be(camps[0].Name);
        result[1].Name.Should().Be(camps[1].Name);

        mockRepo.Verify(r => r.GetAllCampsAsync(false), Times.Once);
    }

    [Fact]
    public async Task GetCamp_WithExistingMoniker_ShouldReturnCampAsync()
    {
        // Arrange
        var camp = new Camp { CampId = 1, Name = "Test Camp", City = "TEST" };

        var mockRepo = new Mock<ICampRepository>();
        mockRepo.Setup(r => r.GetCampAsync("TEST", false)).ReturnsAsync(camp);

        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetCampAsync("TEST");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(camp.Name);
        result.City.Should().Be(camp.City);

        mockRepo.Verify(r => r.GetCampAsync(camp.City, false), Times.Once);
    }

    [Fact]
    public async Task GetCamp_WithNonExistentMoniker_ShouldReturnNullAsync()
    {
        // Arrange
        var mockRepo = new Mock<ICampRepository>();
        mockRepo.Setup(r => r.GetCampAsync(NonExistent, false)).ReturnsAsync((Camp?)null);

        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetCampAsync(NonExistent);

        // Assert
        result.Should().BeNull();

        mockRepo.Verify(r => r.GetCampAsync(NonExistent, false), Times.Once);
    }


    [Fact]
    public async Task GetAllSpeakers_WithSpeakers_ShouldReturnAllAsync()
    {
        // Arrange
        var speakers = new[]
        {
            new Speaker
            {
                SpeakerId = 1,
                FirstName = "John",
                LastName = "Doe",
                MiddleName = "A",
                Company = "Microsoft",
                Twitter = "@johndoe",
                GitHub = "johndoe"
            },
            new Speaker
            {
                SpeakerId = 2,
                FirstName = "Jane",
                LastName = "Smith",
                MiddleName = "B",
                Company = "Google",
                Twitter = "@janesmith",
                GitHub = "janesmith"
            }
        };

        var mockRepo = new Mock<ICampRepository>();
        mockRepo.Setup(r => r.GetAllSpeakersAsync()).ReturnsAsync(speakers);

        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetAllSpeakersAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].FirstName.Should().Be(speakers[0].FirstName);
        result[0].LastName.Should().Be(speakers[0].LastName);
        result[1].FirstName.Should().Be(speakers[1].FirstName);
        result[1].LastName.Should().Be(speakers[1].LastName);

        mockRepo.Verify(r => r.GetAllSpeakersAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllSpeakers_WithNoSpeakers_ShouldReturnEmptyArrayAsync()
    {
        // Arrange
        var mockRepo = new Mock<ICampRepository>();
        mockRepo.Setup(r => r.GetAllSpeakersAsync()).ReturnsAsync(Array.Empty<Speaker>());

        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetAllSpeakersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        mockRepo.Verify(r => r.GetAllSpeakersAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllSpeakers_WhenRepositoryReturnsNull_ShouldReturnEmptyArrayAsync()
    {
        // Arrange
        var mockRepo = new Mock<ICampRepository>();
        mockRepo.Setup(r => r.GetAllSpeakersAsync()).ReturnsAsync((Speaker[])null!);

        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetAllSpeakersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllSpeakersAsync_WhenRepositoryThrows_ShouldPropagateExceptionAsync()
    {
        // Arrange
        var mockRepo = new Mock<ICampRepository>();
        mockRepo.Setup(r => r.GetAllSpeakersAsync())
            .ThrowsAsync(new TimeoutException("Database connection timed out"));

        var service = CreateService(mockRepo);

        // Act
        Func<Task> act = async () => await service.GetAllSpeakersAsync().ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<TimeoutException>()
            .WithMessage("Database connection timed out");
    }

    [Fact]
    public async Task GetAllSpeakers_WithNullOptionalFields_ShouldReturnSpeakersAsync()
    {
        // Arrange
        var speakers = new[]
        {
            new Speaker
            {
                SpeakerId = 1,
                FirstName = "John",
                LastName = "Doe",
                MiddleName = "A",
                Company = "Microsoft",
                Twitter = "@johndoe",
                GitHub = "johndoe"
            }
        };

        var mockRepo = new Mock<ICampRepository>();
        mockRepo.Setup(r => r.GetAllSpeakersAsync()).ReturnsAsync(speakers);

        var service = CreateService(mockRepo);

        // Act
        var result = await service.GetAllSpeakersAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].FirstName.Should().Be(speakers[0].FirstName);
        result[0].LastName.Should().Be(speakers[0].LastName);
        result[0].MiddleName.Should().Be(speakers[0].MiddleName);
        result[0].BlogUrl.Should().Be(speakers[0].BlogUrl);
        result[0].Twitter.Should().Be(speakers[0].Twitter);
        result[0].GitHub.Should().Be(speakers[0].GitHub);
    }

    [Fact]
    public async Task UpdateSpeaker_WithExistingSpeaker_ShouldUpdateAndReturnTrueAsync()
    {
        // Arrange
        var speakerFaker = new Faker<Speaker>();
        var existingSpeaker = speakerFaker.Generate();

        var request = new UpdateSpeakerRequest
        {
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName",
            MiddleName = "UpdatedMiddleName",
            Company = "UpdatedCompany",
            CompanyUrl = "https://updatedcompany.com",
            BlogUrl = "https://updatedblog.com",
            Twitter = "@updatedtwitter",
            GitHub = "updatedgithub"
        };

        var mockRepo = new Mock<ICampRepository>();
        mockRepo.Setup(r => r.GetSpeakerAsync(existingSpeaker.SpeakerId)).ReturnsAsync(existingSpeaker);
        mockRepo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        var service = CreateService(mockRepo);

        // Act
        var result = await service.UpdateSpeakerAsync(existingSpeaker.SpeakerId, request);

        // Assert
        result.Should().BeTrue();
        existingSpeaker.FirstName.Should().Be(request.FirstName);
        existingSpeaker.LastName.Should().Be(request.LastName);
        existingSpeaker.MiddleName.Should().Be(request.MiddleName);
        existingSpeaker.Company.Should().Be(request.Company);
        existingSpeaker.CompanyUrl.Should().Be(request.CompanyUrl);
        existingSpeaker.BlogUrl.Should().Be(request.BlogUrl);
        existingSpeaker.Twitter.Should().Be(request.Twitter);
        existingSpeaker.GitHub.Should().Be(request.GitHub);

        mockRepo.Verify(r => r.GetSpeakerAsync(existingSpeaker.SpeakerId), Times.Once);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateSpeaker_WithNonExistentSpeaker_ShouldReturnFalseAsync()
    {
        // Arrange
        var updateFaker = new Faker<UpdateSpeakerRequest>();
        var request = updateFaker.Generate();

        var mockRepo = new Mock<ICampRepository>();
        mockRepo.Setup(r => r.GetSpeakerAsync(999)).ReturnsAsync((Speaker?)null);

        var service = CreateService(mockRepo);

        // Act
        var result = await service.UpdateSpeakerAsync(999, request);

        // Assert
        result.Should().BeFalse();
        mockRepo.Verify(r => r.GetSpeakerAsync(999), Times.Once);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateSpeaker_WhenRepositoryThrows_ShouldPropagateExceptionAsync()
    {
        // Arrange
        var updateFaker = new Faker<UpdateSpeakerRequest>();
        var request = updateFaker.Generate();

        var mockRepo = new Mock<ICampRepository>();
        mockRepo.Setup(r => r.GetSpeakerAsync(1)).ThrowsAsync(new TimeoutException("Database timed out"));

        var service = CreateService(mockRepo);

        // Act
        Func<Task> act = async () => await service.UpdateSpeakerAsync(1, request).ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<TimeoutException>().WithMessage("Database timed out");
    }
}
