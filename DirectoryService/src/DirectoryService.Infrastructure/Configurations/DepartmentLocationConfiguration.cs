using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations");

        builder.HasKey(x => x.Id);

        builder.Property(d => d.Id)
            .IsRequired()
            .HasColumnName("id");

        builder.Property(x => x.DepartmentId)
            .HasColumnName("department_id")
            .IsRequired();

        builder.Property(x => x.LocationId)
            .HasColumnName("location_id")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");

        builder
            .HasOne<Department>()
            .WithMany(d => d.Locations)
            .HasForeignKey(d => d.DepartmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Location>()
            .WithMany(l => l.Departments)
            .HasForeignKey(d => d.LocationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}