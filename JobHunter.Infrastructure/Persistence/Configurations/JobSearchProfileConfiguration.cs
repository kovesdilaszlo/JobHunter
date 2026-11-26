
using JobHunter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobHunter.Infrastructure.Persistence.Configurations;

public sealed class JobSearchProfileConfiguration
    : IEntityTypeConfiguration<JobSearchProfile>
{
    public void Configure(
        EntityTypeBuilder<JobSearchProfile> builder)
    {
        builder.ToTable("JobSearchProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Keywords)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Location)
            .HasMaxLength(200);

        builder.Property(x => x.IncludeRemote)
            .IsRequired();

        builder.Property(x => x.IsEnabled)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.IsEnabled);
    }
}
