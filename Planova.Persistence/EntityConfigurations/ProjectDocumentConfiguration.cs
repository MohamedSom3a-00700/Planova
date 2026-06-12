using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Planova.Domain.Entities;

namespace Planova.Persistence.EntityConfigurations;

public class ProjectDocumentConfiguration : IEntityTypeConfiguration<ProjectDocument>
{
    public void Configure(EntityTypeBuilder<ProjectDocument> builder)
    {
        builder.ToTable("ProjectDocuments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FileName)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(e => e.RelativePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.DocumentType)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(e => e.FileExtension)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(e => e.UploadedAt)
            .HasDefaultValueSql("datetime('now')");

        builder.HasIndex(e => e.ProjectId)
            .HasDatabaseName("IX_ProjectDocuments_ProjectId");

        builder.HasIndex(new[] { "ProjectId", "DocumentType" })
            .HasDatabaseName("IX_ProjectDocuments_ProjectId_Type");

        builder.HasOne(e => e.Project)
            .WithMany(p => p.Documents)
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
