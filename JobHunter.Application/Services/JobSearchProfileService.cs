
using JobHunter.Application.DTOs;
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Entities;

namespace JobHunter.Application.Services;

public sealed class JobSearchProfileService
    : IJobSearchProfileService
{
    private readonly IJobSearchProfileRepository _repository;

    public JobSearchProfileService(
        IJobSearchProfileRepository repository)
    {
        _repository = repository
            ?? throw new ArgumentNullException(
                nameof(repository));
    }

    public async Task<SearchProfileDto> CreateAsync(
        CreateSearchProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var profile = new JobSearchProfile(
            request.Name,
            request.Keywords,
            request.Location,
            request.IncludeRemote);

        await _repository.AddAsync(
            profile, cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return MapToDto(profile);
    }

    public async Task<SearchProfileDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        ValidateId(id);

        var profile = await _repository.GetByIdAsync(
            id, cancellationToken);

        return profile is null
            ? null
            : MapToDto(profile);
    }

    public async Task<IReadOnlyList<SearchProfileDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var profiles = await _repository.GetAllAsync(
            cancellationToken);

        return profiles.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<SearchProfileDto>> GetEnabledAsync(
        CancellationToken cancellationToken = default)
    {
        var profiles = await _repository.GetEnabledAsync(
            cancellationToken);

        return profiles.Select(MapToDto).ToList();
    }

    public async Task<bool> UpdateAsync(
        Guid id,
        UpdateSearchProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateId(id);
        ArgumentNullException.ThrowIfNull(request);

        var profile = await _repository.GetByIdAsync(
            id, cancellationToken);

        if (profile is null)
            return false;

        profile.Update(
            request.Name,
            request.Keywords,
            request.Location,
            request.IncludeRemote);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> EnableAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        ValidateId(id);

        var profile = await _repository.GetByIdAsync(
            id, cancellationToken);

        if (profile is null)
            return false;

        profile.Enable();

        await _repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> DisableAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        ValidateId(id);

        var profile = await _repository.GetByIdAsync(
            id, cancellationToken);

        if (profile is null)
            return false;

        profile.Disable();

        await _repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        ValidateId(id);

        var profile = await _repository.GetByIdAsync(
            id, cancellationToken);

        if (profile is null)
            return false;

        await _repository.DeleteAsync(
            profile, cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    private static SearchProfileDto MapToDto(
        JobSearchProfile profile)
    {
        return new SearchProfileDto(
            profile.Id,
            profile.Name,
            profile.Keywords,
            profile.Location,
            profile.IncludeRemote,
            profile.IsEnabled,
            profile.CreatedAt,
            profile.UpdatedAt);
    }

    private static void ValidateId(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid search profile ID is required.",
                nameof(id));
        }
    }
}
