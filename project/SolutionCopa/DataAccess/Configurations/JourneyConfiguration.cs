using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCopa;

namespace DataAccess.Configurations;

public class JourneyConfiguration : IEntityTypeConfiguration<Journey>
{
    public void Configure(EntityTypeBuilder<Journey> builder)
    {
        builder.ToTable("Journeys");
        builder.HasKey(j => j.Number);                                  // PK natural; EF materializa con el ctor privado Journey(int number)
        builder.Property(j => j.Number).ValueGeneratedNever();
    }
}
