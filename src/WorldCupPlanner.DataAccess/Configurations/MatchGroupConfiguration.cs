using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Configurations;

public class MatchGroupConfiguration : IEntityTypeConfiguration<MatchGroup>
{
    private const int MatchCodeMaxLength = 20;

    public void Configure(EntityTypeBuilder<MatchGroup> builder)
    {
        // MatchCode comparte columna con MatchElimination en la tabla TPH.
        builder.Property(m => m.MatchCode).HasColumnName("MatchCode").HasMaxLength(MatchCodeMaxLength);

        // Group / Journey: shadow FKs exclusivas de los partidos de grupo, con Restrict
        // (no se puede borrar un grupo/jornada mientras existan partidos que lo referencien;
        // por eso FixtureOrchestrator limpia en orden matches -> journeys -> groups).
        builder.HasOne(m => m.Group)
               .WithMany()
               .HasForeignKey("GroupName")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Journey)
               .WithMany()
               .HasForeignKey("JourneyNumber")
               .OnDelete(DeleteBehavior.Restrict);
    }
}
