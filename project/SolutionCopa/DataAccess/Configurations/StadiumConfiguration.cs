using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCopa;

namespace DataAccess.Configurations;

public class StadiumConfiguration : IEntityTypeConfiguration<Stadium>
{
    private const int NameMaxLength = 80;
    private const int CityMaxLength = 60;
    private const int DescriptionMaxLength = 400;

    public void Configure(EntityTypeBuilder<Stadium> builder)
    {
        builder.ToTable("Stadiums");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();              // Id asignado por StadiumServices (max+1), no IDENTITY
        builder.Property(s => s.Name).IsRequired().HasMaxLength(NameMaxLength);
        builder.HasIndex(s => s.Name).IsUnique();                       // Name unico (regla de negocio)
        builder.Property(s => s.City).IsRequired().HasMaxLength(CityMaxLength);
        builder.Property(s => s.Description).HasMaxLength(DescriptionMaxLength);  // opcional
        builder.Property(s => s.Capacity).IsRequired();
    }
}
