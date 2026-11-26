
using JobHunter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace JobHunter.Infrastructure.Persistence;

public sealed class JobHunterDbContext : DbContext
{
    public JobHunterDbContext(
        DbContextOptions<JobHunterDbContext> options)
        : base(options)
    {
    }

    public DbSet<JobPosting> JobPostings =>
        Set<JobPosting>();

    public DbSet<JobPostingSource> JobPostingSources =>
        Set<JobPostingSource>();

    public DbSet<JobApplication> JobApplications =>
        Set<JobApplication>();

    public DbSet<JobSearchProfile> JobSearchProfiles =>
        Set<JobSearchProfile>();

    public DbSet<SearchExecution> SearchExecutions =>
        Set<SearchExecution>();

    public DbSet<ApiQuotaUsage> ApiQuotaUsages =>
        Set<ApiQuotaUsage>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(JobHunterDbContext).Assembly);

        if (Database.ProviderName ==
            "Microsoft.EntityFrameworkCore.Sqlite")
        {
            ConfigureSqlite(modelBuilder);
        }
    }

    private static void ConfigureSqlite(
        ModelBuilder modelBuilder)
    {
        var converter =
            new ValueConverter<DateTimeOffset, long>(
                value => value.UtcDateTime.Ticks,
                value => new DateTimeOffset(
                    value, TimeSpan.Zero));

        foreach (var entityType in
                 modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in
                     entityType.GetProperties())
            {
                if (property.ClrType ==
                        typeof(DateTimeOffset) ||
                    property.ClrType ==
                        typeof(DateTimeOffset?))
                {
                    property.SetValueConverter(converter);
                }
            }
        }
    }
}
