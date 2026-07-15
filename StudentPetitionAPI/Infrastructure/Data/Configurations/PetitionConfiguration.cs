using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentPetitionAPI.Domain.Entities;

namespace StudentPetitionAPI.Infrastructure.Data.Configurations;

public class PetitionConfiguration : IEntityTypeConfiguration<Petition>
{
    public void Configure(EntityTypeBuilder<Petition> builder)
    {
        builder.ToTable("Petitions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.StudentId)
            .IsRequired();

        builder.Property(p => p.PetitionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        builder.Property(p => p.ReviewedBy)
            .HasMaxLength(100);

        builder.Property(p => p.ReviewedAt);

        builder.Property(p => p.ReviewComment)
            .HasMaxLength(2000);

        builder.HasIndex(p => new { p.StudentId, p.CreatedAt })
            .HasDatabaseName("IX_Petitions_StudentId_CreatedAt");

        builder.HasIndex(p => new { p.Status, p.CreatedAt })
            .HasDatabaseName("IX_Petitions_Status_CreatedAt");

        builder.HasIndex(p => new { p.PetitionType, p.CreatedAt })
            .HasDatabaseName("IX_Petitions_PetitionType_CreatedAt");
    }
}
