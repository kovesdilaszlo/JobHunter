
using JobHunter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobHunter.Infrastructure.Persistence.Configurations;

public sealed class ApiQuotaUsageConfiguration
    : IEntityTypeConfiguration<ApiQuotaUsage>
{
    public void Configure(
        EntityTypeBuilder<ApiQuotaUsage> builder)
    {
        builder.ToTable("ApiQuotaUsages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SourceType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.UsageDate)
            .IsRequired();

        builder.Property(x => x.RequestCount)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.SourceType,
            x.UsageDate
        })
        .IsUnique();

        builder.HasIndex(x => x.UsageDate);
    }
}
