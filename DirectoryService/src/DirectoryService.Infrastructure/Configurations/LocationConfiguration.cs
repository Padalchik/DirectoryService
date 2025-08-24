using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");

        builder.HasKey(l => l.Id);

        builder.Property(d => d.Id)
            .IsRequired()
            .HasColumnName("id");

        builder.ComplexProperty(l => l.Name, li =>
        {
            li.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(Constants.MAX_LOCATION_NAME_LENGTH)
                .HasColumnName("name");
        });

        builder.ComplexProperty(l => l.Address, lb =>
        {
            lb.Property(pr => pr.City)
                .IsRequired()
                .HasColumnName("city");

            lb.Property(pr => pr.Street)
                .IsRequired()
                .HasColumnName("street");

            lb.Property(pr => pr.HouseNumber)
                .IsRequired()
                .HasColumnName("house_number");
        });

        builder.ComplexProperty(l => l.Timezone, li =>
        {
            li.Property(d => d.Value)
                .IsRequired()
                .HasColumnName("time_zone");
        });

        builder.Property(d => d.IsActive)
            .HasColumnName("is_active");

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at");
    }
}