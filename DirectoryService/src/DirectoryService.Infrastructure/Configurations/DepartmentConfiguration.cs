using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .IsRequired()
            .HasColumnName("id");

        builder.HasOne(d => d.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(d => d.ParentId);

        builder.Property(d => d.ParentId)
            .IsRequired(false)
            .HasColumnName("parent_id");

        builder.ComplexProperty(d => d.Name, di =>
        {
            di.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(Constants.MAX_DEPARTMENT_NAME_LENGTH)
                .HasColumnName("name");
        });

        builder.OwnsOne(d => d.Identifier, ib =>
        {
            ib.Property(x => x.Identifier)
                .HasColumnName("identifier")
                .HasMaxLength(Constants.MAX_DEPARTMENT_NAME_LENGTH)
                .IsRequired();

            ib.HasIndex(x => x.Identifier).IsUnique();
        });

        builder.Property(d => d.Path)
            .HasColumnName("path")
            .HasColumnType("ltree");

        builder.HasIndex(d => d.Path).HasMethod("gist").HasDatabaseName("idx_departments_path");

        builder.Property(d => d.Depth)
            .HasColumnName("depth");

        builder.Property(d => d.IsActive)
            .HasColumnName("is_active");

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Ignore(d => d.ChildrenCount);

        builder.HasMany(d => d.Locations)
            .WithOne()
            .HasForeignKey(dl => dl.DepartmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Positions)
            .WithOne()
            .HasForeignKey(dl => dl.DepartmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(d => d.DeletedAt)
            .HasColumnName("deleted_at");
    }
}