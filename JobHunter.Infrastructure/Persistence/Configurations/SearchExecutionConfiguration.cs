
using JobHunter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobHunter.Infrastructure.Persistence.Configurations;

public sealed class SearchExecutionConfiguration
    : IEntityTypeConfiguration<SearchExecution>
{
    public void Configure(
        EntityTypeBuilder<SearchExecution> builder)
    {
        builder.ToTable("SearchExecutions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProviderName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.TriggerType)
            .HasConversion<int>();

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasOne<JobSearchProfile>()
            .WithMany()
            .HasForeignKey(x => x.JobSearchProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.StartedAt);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.JobSearchProfileId);
    }
}
