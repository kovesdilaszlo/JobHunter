using JobHunter.Domain.Entities;
using JobHunter.Domain.Enums;
using JobHunter.Infrastructure.Repositories;
using JobHunter.Test.Integration;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JobHunter.Test.Infrastructure.Repositories;

public class JobRepositoryIntegrationTests
{
    private static JobPosting CreateJob()
    {
        var job = new JobPosting(
            "Junior .NET Developer",
            "Example Company",
            "Budapest",
            "ASP.NET Core developer position",
            WorkArrangement.Hybrid);

        job.AddSource(new JobPostingSource(
            JobSourceType.Jooble,
            "https://example.com/jobs/123",
            "123"));

        return job;
    }

    [Fact]
    public async Task AddAsync_ValidJob_PersistsToDatabase()
    {
        using var database = new SqliteTestDatabase();

        var job = CreateJob();

        await using (var context = database.CreateContext())
        {
            var repository = new JobRepository(context);

            await repository.AddAsync(job);

            await repository.SaveChangesAsync();
        }

        await using (var context = database.CreateContext())
        {
            var repository = new JobRepository(context);

            var result = await repository.GetByIdAsync(job.Id);

            Assert.NotNull(result);

            Assert.Equal(job.Id, result.Id);
            Assert.Equal(job.Title, result.Title);
            Assert.Equal(job.Company, result.Company);
            Assert.Equal(job.Location, result.Location);
        }
    }

    [Fact]
    public async Task AddAsync_JobWithSource_PersistsRelationship()
    {
        using var database = new SqliteTestDatabase();

        var job = CreateJob();

        await using (var context = database.CreateContext())
        {
            var repository = new JobRepository(context);

            await repository.AddAsync(job);

            await repository.SaveChangesAsync();
        }

        await using (var context = database.CreateContext())
        {
            var repository = new JobRepository(context);

            var result = await repository.GetByIdAsync(job.Id);

            Assert.NotNull(result);

            var source = Assert.Single(result.Sources);

            Assert.Equal(job.Id, source.JobPostingId);

            Assert.Equal(
                JobSourceType.Jooble,
                source.SourceType);

            Assert.Equal("123", source.ExternalId);

            Assert.Equal(64, source.UrlFingerprint.Length);
        }
    }

    [Fact]
    public async Task GetByIdAsync_UnknownJob_ReturnsNull()
    {
        using var database = new SqliteTestDatabase();

        await using var context = database.CreateContext();

        var repository = new JobRepository(context);

        var result = await repository.GetByIdAsync(
            Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_MultipleJobs_ReturnsAll()
    {
        using var database = new SqliteTestDatabase();

        await using var context = database.CreateContext();

        var repository = new JobRepository(context);

        var first = CreateJob();

        var second = new JobPosting(
            "BI Analyst",
            "Another Company",
            "Budapest",
            "Power BI and SQL",
            WorkArrangement.Remote);

        second.AddSource(new JobPostingSource(
            JobSourceType.LinkedIn,
            "https://example.com/jobs/456",
            "456"));

        await repository.AddAsync(first);

        await repository.AddAsync(second);

        await repository.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var results = await repository.GetAllAsync();

        Assert.Equal(2, results.Count);

        Assert.Contains(results, x => x.Id == first.Id);

        Assert.Contains(results, x => x.Id == second.Id);
    }

    [Fact]
    public async Task ChangeStatusAsync_PersistsAfterReload()
    {
        using var database = new SqliteTestDatabase();

        var job = CreateJob();

        await using (var context = database.CreateContext())
        {
            var repository = new JobRepository(context);

            await repository.AddAsync(job);

            await repository.SaveChangesAsync();
        }

        await using (var context = database.CreateContext())
        {
            var repository = new JobRepository(context);

            var loaded = await repository.GetByIdAsync(job.Id);

            Assert.NotNull(loaded);

            loaded.ChangeStatus(ApplicationStatus.Applied);

            await repository.SaveChangesAsync();
        }

        await using (var context = database.CreateContext())
        {
            var repository = new JobRepository(context);

            var reloaded =
                await repository.GetByIdAsync(job.Id);

            Assert.NotNull(reloaded);

            Assert.Equal(
                ApplicationStatus.Applied,
                reloaded.Status);
        }
    }

    [Fact]
    public async Task AddAsync_DuplicateExternalId_ThrowsDbUpdateException()
    {
        using var database = new SqliteTestDatabase();

        await using var context = database.CreateContext();

        var repository = new JobRepository(context);

        await repository.AddAsync(CreateJob());

        await repository.SaveChangesAsync();

        var duplicate = new JobPosting(
            "Another Developer",
            "Different Company",
            "Pecs",
            "Different job",
            WorkArrangement.OnSite);

        duplicate.AddSource(new JobPostingSource(
            JobSourceType.Jooble,
            "https://other.com/jobs/999",
            "123"));

        await repository.AddAsync(duplicate);

        await Assert.ThrowsAsync<DbUpdateException>(
            () => repository.SaveChangesAsync());
    }
}
