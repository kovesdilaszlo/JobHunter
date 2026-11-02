
using JobHunter.Application.DTOs;
using JobHunter.Application.Interfaces;
using JobHunter.Application.Services;
using JobHunter.Domain.Entities;

namespace JobHunter.Test.Application.Services;

public class JobSearchProfileServiceTests
{
    private static CreateSearchProfileRequest CreateRequest()
    {
        return new CreateSearchProfileRequest(
            "Junior .NET Developer",
            "C#, ASP.NET Core",
            "Budapest",
            true);
    }

    [Fact]
    public void Constructor_NullRepository_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new JobSearchProfileService(null!));
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_SavesProfile()
    {
        var repository = new FakeSearchProfileRepository();
        var service = new JobSearchProfileService(repository);

        var result = await service.CreateAsync(CreateRequest());

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Junior .NET Developer", result.Name);
        Assert.Equal("C#, ASP.NET Core", result.Keywords);
        Assert.Equal("Budapest", result.Location);
        Assert.True(result.IncludeRemote);
        Assert.True(result.IsEnabled);

        Assert.Single(repository.Profiles);
        Assert.Equal(1, repository.SaveChangesCount);
    }

    [Fact]
    public async Task CreateAsync_NullRequest_ThrowsException()
    {
        var service = new JobSearchProfileService(
            new FakeSearchProfileRepository());

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.CreateAsync(null!));
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ThrowsException()
    {
        var service = new JobSearchProfileService(
            new FakeSearchProfileRepository());

        var request = CreateRequest() with { Name = "" };

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(request));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingProfile_ReturnsProfile()
    {
        var service = new JobSearchProfileService(
            new FakeSearchProfileRepository());

        var created = await service.CreateAsync(CreateRequest());

        var result = await service.GetByIdAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownProfile_ReturnsNull()
    {
        var service = new JobSearchProfileService(
            new FakeSearchProfileRepository());

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_EmptyId_ThrowsException()
    {
        var service = new JobSearchProfileService(
            new FakeSearchProfileRepository());

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetByIdAsync(Guid.Empty));
    }

    [Fact]
    public async Task GetAllAsync_MultipleProfiles_ReturnsAll()
    {
        var service = new JobSearchProfileService(
            new FakeSearchProfileRepository());

        await service.CreateAsync(CreateRequest());

        await service.CreateAsync(
            CreateRequest() with
            {
                Name = "Data Analyst",
                Keywords = "SQL, Power BI"
            });

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetEnabledAsync_DisabledProfile_ExcludesProfile()
    {
        var service = new JobSearchProfileService(
            new FakeSearchProfileRepository());

        var first = await service.CreateAsync(CreateRequest());

        await service.CreateAsync(
            CreateRequest() with
            {
                Name = "Data Analyst",
                Keywords = "SQL"
            });

        await service.DisableAsync(first.Id);

        var result = await service.GetEnabledAsync();

        Assert.Single(result);
        Assert.Equal("Data Analyst", result[0].Name);
    }

    [Fact]
    public async Task UpdateAsync_ExistingProfile_UpdatesValues()
    {
        var repository = new FakeSearchProfileRepository();
        var service = new JobSearchProfileService(repository);

        var created = await service.CreateAsync(CreateRequest());

        var updated = await service.UpdateAsync(
            created.Id,
            new UpdateSearchProfileRequest(
                "BI Analyst",
                "Power BI, SQL",
                "Hungary",
                false));

        var result = await service.GetByIdAsync(created.Id);

        Assert.True(updated);
        Assert.NotNull(result);
        Assert.Equal("BI Analyst", result.Name);
        Assert.Equal("Power BI, SQL", result.Keywords);
        Assert.Equal("Hungary", result.Location);
        Assert.False(result.IncludeRemote);
        Assert.Equal(2, repository.SaveChangesCount);
    }

    [Fact]
    public async Task UpdateAsync_UnknownProfile_ReturnsFalse()
    {
        var service = new JobSearchProfileService(
            new FakeSearchProfileRepository());

        var updated = await service.UpdateAsync(
            Guid.NewGuid(),
            new UpdateSearchProfileRequest(
                "Developer",
                "C#",
                "Budapest",
                true));

        Assert.False(updated);
    }

    [Fact]
    public async Task UpdateAsync_InvalidData_ThrowsException()
    {
        var service = new JobSearchProfileService(
            new FakeSearchProfileRepository());

        var created = await service.CreateAsync(CreateRequest());

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateAsync(
                created.Id,
                new UpdateSearchProfileRequest(
                    "",
                    "SQL",
                    "Budapest",
                    true)));
    }

    [Fact]
    public async Task DisableAsync_ExistingProfile_DisablesProfile()
    {
        var service = new JobSearchProfileService(
            new FakeSearchProfileRepository());

        var created = await service.CreateAsync(CreateRequest());

        var disabled = await service.DisableAsync(created.Id);
        var result = await service.GetByIdAsync(created.Id);

        Assert.True(disabled);
        Assert.NotNull(result);
        Assert.False(result.IsEnabled);
    }

    [Fact]
    public async Task EnableAsync_DisabledProfile_EnablesProfile()
    {
        var service = new JobSearchProfileService(
            new FakeSearchProfileRepository());

        var created = await service.CreateAsync(CreateRequest());

        await service.DisableAsync(created.Id);

        var enabled = await service.EnableAsync(created.Id);
        var result = await service.GetByIdAsync(created.Id);

        Assert.True(enabled);
        Assert.NotNull(result);
        Assert.True(result.IsEnabled);
    }

    [Fact]
    public async Task DeleteAsync_ExistingProfile_RemovesProfile()
    {
        var repository = new FakeSearchProfileRepository();
        var service = new JobSearchProfileService(repository);

        var created = await service.CreateAsync(CreateRequest());

        var deleted = await service.DeleteAsync(created.Id);

        Assert.True(deleted);
        Assert.Empty(repository.Profiles);
        Assert.Equal(2, repository.SaveChangesCount);
    }

    [Fact]
    public async Task DeleteAsync_UnknownProfile_ReturnsFalse()
    {
        var service = new JobSearchProfileService(
            new FakeSearchProfileRepository());

        var deleted = await service.DeleteAsync(Guid.NewGuid());

        Assert.False(deleted);
    }

    [Fact]
    public async Task DisableAsync_UnknownProfile_ReturnsFalse()
    {
        var service = new JobSearchProfileService(
            new FakeSearchProfileRepository());

        var disabled = await service.DisableAsync(Guid.NewGuid());

        Assert.False(disabled);
    }

    private sealed class FakeSearchProfileRepository
        : IJobSearchProfileRepository
    {
        public List<JobSearchProfile> Profiles { get; } = new();

        public int SaveChangesCount { get; private set; }

        public Task<JobSearchProfile?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Profiles.FirstOrDefault(p => p.Id == id));
        }

        public Task<IReadOnlyList<JobSearchProfile>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<JobSearchProfile>>(
                Profiles.ToList());
        }

        public Task<IReadOnlyList<JobSearchProfile>> GetEnabledAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<JobSearchProfile>>(
                Profiles.Where(p => p.IsEnabled).ToList());
        }

        public Task AddAsync(
            JobSearchProfile profile,
            CancellationToken cancellationToken = default)
        {
            Profiles.Add(profile);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(
            JobSearchProfile profile,
            CancellationToken cancellationToken = default)
        {
            Profiles.Remove(profile);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCount++;
            return Task.CompletedTask;
        }
    }
}
