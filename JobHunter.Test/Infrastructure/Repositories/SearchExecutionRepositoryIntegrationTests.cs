using JobHunter.Domain.Entities;
using JobHunter.Domain.Enums;
using JobHunter.Infrastructure.Repositories;
using JobHunter.Test.Integration;
using Xunit;

namespace JobHunter.Test.Infrastructure.Repositories;

public class SearchExecutionRepositoryIntegrationTests
{
    private static JobSearchProfile CreateProfile()
    {
        return new JobSearchProfile(
            "Junior Developer",
            "C#",
            "Budapest",
            true);
    }

    [Fact]
    public async Task AddAsync_ValidExecution_PersistsToDatabase()
    {
        using var database = new SqliteTestDatabase();

        var profile = CreateProfile();

        var execution = new SearchExecution(
            profile.Id,
            "Jooble",
            SearchTriggerType.Manual);

        execution.Start();

        execution.Complete(10, 8, 2);

        await using (var context = database.CreateContext())
        {
            var profileRepository =
                new JobSearchProfileRepository(context);

            var executionRepository =
                new SearchExecutionRepository(context);

            await profileRepository.AddAsync(profile);

            await executionRepository.AddAsync(execution);

            await executionRepository.SaveChangesAsync();
        }

        await using (var context = database.CreateContext())
        {
            var repository =
                new SearchExecutionRepository(context);

            var result = await repository.GetByIdAsync(
                execution.Id);

            Assert.NotNull(result);

            Assert.Equal(execution.Id, result.Id);

            Assert.Equal(
                profile.Id,
                result.JobSearchProfileId);

            Assert.Equal("Jooble", result.ProviderName);

            Assert.Equal(
                SearchExecutionStatus.Completed,
                result.Status);

            Assert.Equal(10, result.RetrievedCount);

            Assert.Equal(8, result.InsertedCount);

            Assert.Equal(2, result.DuplicateCount);
        }
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsRequestedCount()
    {
        using var database = new SqliteTestDatabase();

        await using var context = database.CreateContext();

        var profile = CreateProfile();

        var profileRepository =
            new JobSearchProfileRepository(context);

        var repository =
            new SearchExecutionRepository(context);

        await profileRepository.AddAsync(profile);

        for (int i = 0; i < 5; i++)
        {
            var execution = new SearchExecution(
                profile.Id,
                "Jooble",
                SearchTriggerType.Scheduled);

            execution.Start();

            await repository.AddAsync(execution);
        }

        await repository.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var results = await repository.GetRecentAsync(3);

        Assert.Equal(3, results.Count);

        Assert.All(results, execution =>
        {
            Assert.Equal(
                profile.Id,
                execution.JobSearchProfileId);
        });
    }

    [Fact]
    public async Task GetRecentAsync_InvalidCount_ThrowsException()
    {
        using var database = new SqliteTestDatabase();

        await using var context = database.CreateContext();

        var repository =
            new SearchExecutionRepository(context);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => repository.GetRecentAsync(0));
    }
}
