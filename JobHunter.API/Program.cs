
using JobHunter.Application.Interfaces;
using JobHunter.Application.Services;
using JobHunter.Infrastructure;
using JobHunter.Infrastructure.Concurrency;

var builder = WebApplication.CreateBuilder(args);

// Controllers

builder.Services.AddControllers();

// Infrastructure

builder.Services.AddInfrastructure(
    builder.Configuration);

// Application services

builder.Services.AddScoped<
    IJobService,
    JobService>();

builder.Services.AddScoped<
    IDuplicateDetectionService,
    DuplicateDetectionService>();

builder.Services.AddScoped<
    IJobSearchProfileService,
    JobSearchProfileService>();

// Import concurrency

builder.Services.AddSingleton<
    IJobImportLock,
    JobImportLock>();

// Original import service

builder.Services.AddScoped<JobImportService>();

// Synchronized import service

builder.Services.AddScoped<IJobImportService>(
    serviceProvider =>
        new SynchronizedJobImportService(
            serviceProvider
                .GetRequiredService<JobImportService>(),
            serviceProvider
                .GetRequiredService<IJobImportLock>()));

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/", () =>
    Results.Ok(new
    {
        Application = "JobHunter API",
        Status = "Running"
    }));

app.Run();

public partial class Program { }
