using DirectoryService.Domain.Department;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(d => d.Id);

        builder.HasOne(d => d.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey("parent");

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

        builder.ComplexProperty(d => d.Identifier, di =>
        {
            di.Property(d => d.Identifier)
                .IsRequired()
                .HasMaxLength(Constants.MAX_DEPARTMENT_NAME_LENGTH)
                .HasColumnName("identifier");
        });

        builder.Property(d => d.Path)
            .HasColumnName("path");

        builder.Property(d => d.Depth)
            .HasColumnName("depth");

        builder.Property(d => d.IsActive)
            .HasColumnName("is_active");

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(d => d.ChildrenCount)
            .HasColumnName("children_count");
        
        //DepartmentLocation
        //DepartmentPosition

        //builder.OwnsMany(q => q.Locations);
    }
}