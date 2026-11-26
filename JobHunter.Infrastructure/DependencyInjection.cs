
using JobHunter.Application.Interfaces;
using JobHunter.Infrastructure.Persistence;
using JobHunter.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobHunter.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("JobHunterDb")
            ?? throw new InvalidOperationException(
                "Connection string 'JobHunterDb' is missing.");

        services.AddDbContext<JobHunterDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IJobRepository, JobRepository>();

        services.AddScoped<
            IJobSearchProfileRepository,
            JobSearchProfileRepository>();

        services.AddScoped<
            ISearchExecutionRepository,
            SearchExecutionRepository>();

        return services;
    }
}
