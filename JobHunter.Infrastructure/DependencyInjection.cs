
using JobHunter.Application.Interfaces;
using JobHunter.Infrastructure.Persistence;
using JobHunter.Infrastructure.Providers.Jooble;
using JobHunter.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

        // Database

        services.AddDbContext<JobHunterDbContext>(
            options =>
                options.UseSqlServer(connectionString));

        // Repositories

        services.AddScoped<
            IJobRepository,
            JobRepository>();

        services.AddScoped<
            IJobSearchProfileRepository,
            JobSearchProfileRepository>();

        services.AddScoped<
            ISearchExecutionRepository,
            SearchExecutionRepository>();

        // Jooble configuration

        services.Configure<JoobleOptions>(
            configuration.GetSection(
                JoobleOptions.SectionName));

        // Jooble HTTP client

        services.AddHttpClient<JoobleJobProvider>(
            (serviceProvider, client) =>
            {
                var options = serviceProvider
                    .GetRequiredService<
                        IOptions<JoobleOptions>>()
                    .Value;

                client.BaseAddress =
                    new Uri("https://hu.jooble.org/");

                client.Timeout =
                    TimeSpan.FromSeconds(
                        options.TimeoutSeconds);
            })
            .ConfigurePrimaryHttpMessageHandler(
                () => new HttpClientHandler
                {
                    AllowAutoRedirect = false
                })
            .RemoveAllLoggers();

        // Job providers

        services.AddScoped<IJobProvider>(
            serviceProvider =>
                serviceProvider.GetRequiredService<
                    JoobleJobProvider>());

        return services;
    }
}
