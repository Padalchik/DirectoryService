using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .IsRequired()
            .HasColumnName("id");

        builder.ComplexProperty(l => l.Name, li =>
        {
            li.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(Constants.MAX_POSITION_NAME_LENGTH)
                .HasColumnName("name");
        });

        builder.ComplexProperty(l => l.Description, li =>
        {
            li.Property(d => d.Value)
                .IsRequired()
                .HasMaxLength(Constants.MAX_POSITION_DESCRIPTION_LENGTH)
                .HasColumnName("description");
        });

        builder.Property(d => d.IsActive)
            .HasColumnName("is_active");

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasMany(d => d.Departments)
            .WithOne()
            .HasForeignKey(dl => dl.DepartmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}