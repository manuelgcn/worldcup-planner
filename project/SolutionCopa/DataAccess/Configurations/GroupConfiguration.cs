using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCopa;

namespace DataAccess.Configurations;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    private const int NameMaxLength = 3;                                // etiquetas "A".."L"

    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("Groups");
        builder.HasKey(g => g.Name);                                    // PK natural: Group no tiene Id
        builder.Property(g => g.Name).ValueGeneratedNever().HasMaxLength(NameMaxLength);

        // Group expone Teams como IReadOnlyCollection sobre el backing field privado `teams`.
        // Team no navega de vuelta: la relacion es una shadow FK "GroupName" (nullable) en la
        // tabla Teams. Borrar un grupo libera a sus equipos (SetNull), no los elimina.
        builder.HasMany(g => g.Teams)
               .WithOne()
               .HasForeignKey("GroupName")
               .OnDelete(DeleteBehavior.SetNull);
        builder.Navigation(g => g.Teams)
               .HasField("teams")
               .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
