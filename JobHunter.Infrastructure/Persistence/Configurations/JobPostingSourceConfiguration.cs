
using JobHunter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobHunter.Infrastructure.Persistence.Configurations;

public sealed class JobPostingSourceConfiguration
    : IEntityTypeConfiguration<JobPostingSource>
{
    public void Configure(
        EntityTypeBuilder<JobPostingSource> builder)
    {
        builder.ToTable("JobPostingSources");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SourceType)
            .HasConversion<int>();

        builder.Property(x => x.ExternalId)
            .HasMaxLength(450);

        builder.Property(x => x.OriginalUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(x => x.NormalizedUrl)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(x => x.UrlFingerprint)
            .IsRequired()
            .HasMaxLength(64)
            .IsUnicode(false);

        builder.Property(x => x.DiscoveredAt)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.SourceType,
            x.ExternalId
        })
        .IsUnique()
        .HasFilter("[ExternalId] IS NOT NULL");

        builder.HasIndex(x => new
        {
            x.SourceType,
            x.UrlFingerprint
        })
        .IsUnique();

        builder.HasIndex(x => x.UrlFingerprint);

        builder.HasIndex(x => x.JobPostingId);
    }
}
