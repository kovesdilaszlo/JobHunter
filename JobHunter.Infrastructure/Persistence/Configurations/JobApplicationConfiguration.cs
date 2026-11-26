
using JobHunter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobHunter.Infrastructure.Persistence.Configurations;

public sealed class JobApplicationConfiguration
    : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(
        EntityTypeBuilder<JobApplication> builder)
    {
        builder.ToTable("JobApplications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Notes);

        builder.Property(x => x.LastUpdatedAt)
            .IsRequired();

        builder.HasOne<JobPosting>()
            .WithOne()
            .HasForeignKey<JobApplication>(
                x => x.JobPostingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.JobPostingId)
            .IsUnique();
    }
}
