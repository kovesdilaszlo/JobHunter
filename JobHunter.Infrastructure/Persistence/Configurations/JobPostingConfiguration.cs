
using JobHunter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobHunter.Infrastructure.Persistence.Configurations;

public sealed class JobPostingConfiguration
    : IEntityTypeConfiguration<JobPosting>
{
    public void Configure(
        EntityTypeBuilder<JobPosting> builder)
    {
        builder.ToTable("JobPostings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.Company)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Location)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.WorkArrangement)
            .HasConversion<int>();

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.Property(x => x.DiscoveredAt)
            .IsRequired();

        builder.Property(x => x.IsArchived)
            .IsRequired();

        builder.HasMany(x => x.Sources)
            .WithOne()
            .HasForeignKey(x => x.JobPostingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Sources)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.DiscoveredAt);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.IsArchived);
    }
}
