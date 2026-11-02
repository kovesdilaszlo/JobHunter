
using JobHunter.Domain.Enums;

namespace JobHunter.Application.DTOs;

public sealed record UpdateJobStatusRequest(
    ApplicationStatus Status);
