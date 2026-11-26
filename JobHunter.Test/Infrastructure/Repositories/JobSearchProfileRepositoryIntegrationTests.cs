using JobHunter.Domain.Entities;
using JobHunter.Infrastructure.Repositories;
using JobHunter.Test.Integration;
using Xunit;

namespace JobHunter.Test.Infrastructure.Repositories;

public class JobSearchProfileRepositoryIntegrationTests
{
    private static JobSearchProfile CreateProfile(
        string name = "Junior .NET Developer")
    {
        return new JobSearchProfile(
            name,
            "C#, ASP.NET Core",
            "Budapest",
            true);
    }

    [Fact]
    public async Task AddAsync_ValidProfile_PersistsToDatabase()
    {
        using var database = new SqliteTestDatabase();

        var profile = CreateProfile();

        await using (var context = database.CreateContext())
        {
            var repository =
                new JobSearchProfileRepository(context);

            await repository.AddAsync(profile);

            await repository.SaveChangesAsync();
        }

        await using (var context = database.CreateContext())
        {
            var repository =
                new JobSearchProfileRepository(context);

            var result =
                await repository.GetByIdAsync(profile.Id);

            Assert.NotNull(result);

            Assert.Equal(profile.Id, result.Id);
            Assert.Equal(profile.Name, result.Name);
            Assert.Equal(profile.Keywords, result.Keywords);
            Assert.Equal(profile.Location, result.Location);

            Assert.True(result.IsEnabled);
            Assert.True(result.IncludeRemote);
        }
    }

    [Fact]
    public async Task GetEnabledAsync_ReturnsOnlyEnabledProfiles()
    {
        using var database = new SqliteTestDatabase();

        await using var context = database.CreateContext();

        var repository =
            new JobSearchProfileRepository(context);

        var enabled = CreateProfile();

        var disabled = CreateProfile("Data Analyst");

        disabled.Disable();

        await repository.AddAsync(enabled);

        await repository.AddAsync(disabled);

        await repository.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var results = await repository.GetEnabledAsync();

        var result = Assert.Single(results);

        Assert.Equal(enabled.Id, result.Id);

        Assert.True(result.IsEnabled);
    }

    [Fact]
    public async Task UpdateAsync_ChangesPersistAfterReload()
    {
        using var database = new SqliteTestDatabase();

        var profile = CreateProfile();

        await using (var context = database.CreateContext())
        {
            var repository =
                new JobSearchProfileRepository(context);

            await repository.AddAsync(profile);

            await repository.SaveChangesAsync();
        }

        await using (var context = database.CreateContext())
        {
            var repository =
                new JobSearchProfileRepository(context);

            var loaded =
                await repository.GetByIdAsync(profile.Id);

            Assert.NotNull(loaded);

            loaded.Update(
                "BI Analyst",
                "SQL, Power BI",
                "Hungary",
                false);

            await repository.SaveChangesAsync();
        }

        await using (var context = database.CreateContext())
        {
            var repository =
                new JobSearchProfileRepository(context);

            var result =
                await repository.GetByIdAsync(profile.Id);

            Assert.NotNull(result);

            Assert.Equal("BI Analyst", result.Name);

            Assert.Equal("SQL, Power BI", result.Keywords);

            Assert.Equal("Hungary", result.Location);

            Assert.False(result.IncludeRemote);
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesProfile()
    {
        using var database = new SqliteTestDatabase();

        var profile = CreateProfile();

        await using (var context = database.CreateContext())
        {
            var repository =
                new JobSearchProfileRepository(context);

            await repository.AddAsync(profile);

            await repository.SaveChangesAsync();

            await repository.DeleteAsync(profile);

            await repository.SaveChangesAsync();
        }

        await using (var context = database.CreateContext())
        {
            var repository =
                new JobSearchProfileRepository(context);

            var result =
                await repository.GetByIdAsync(profile.Id);

            Assert.Null(result);
        }
    }
}
