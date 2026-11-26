
using JobHunter.Application.Interfaces;
using JobHunter.Application.Services;
using JobHunter.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// API controllers
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

builder.Services.AddScoped<
    IJobImportService,
    JobImportService>();

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
