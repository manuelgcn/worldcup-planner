using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Configurations;

// Configuracion base de la jerarquia Match con TPH (Table-per-Hierarchy):
// una unica tabla "Matches" con discriminador "MatchType". Las subclases
// (MatchGroup / MatchElimination) agregan sus propios mapeos en configuraciones dedicadas.
public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");
        builder.HasKey(m => m.MatchId);
        builder.Property(m => m.MatchId).ValueGeneratedNever();         // MatchId lo asigna la app, no IDENTITY

        builder.Property(m => m.MatchDate).IsRequired();
        builder.Property(m => m.HomeGoals).IsRequired();
        builder.Property(m => m.AwayGoals).IsRequired();
        builder.Property(m => m.WasSimulated).IsRequired();
        builder.Property(m => m.HasResult).IsRequired();
        builder.Property(m => m.RankingApplied).IsRequired();
        builder.Property(m => m.AppliedHomeRankingDelta).IsRequired();
        builder.Property(m => m.AppliedAwayRankingDelta).IsRequired();

        // TPH: el tipo concreto se resuelve por el discriminador.
        builder.HasDiscriminator<string>("MatchType")
               .HasValue<MatchGroup>("Group")
               .HasValue<MatchElimination>("Elimination");

        // HomeTeam / AwayTeam: shadow FKs nullables con Restrict (no se borra un equipo
        // referenciado). Acceso por backing field para no disparar los setters con validacion.
        builder.HasOne(m => m.HomeTeam)
               .WithMany()
               .HasForeignKey("HomeTeamId")
               .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(m => m.HomeTeam)
               .HasField("_homeTeam")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(m => m.AwayTeam)
               .WithMany()
               .HasForeignKey("AwayTeamId")
               .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(m => m.AwayTeam)
               .HasField("_awayTeam")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Stadium: shadow FK nullable, Restrict (borrar un estadio referenciado por un
        // partido debe fallar de forma controlada, no borrar el partido en cascada).
        builder.HasOne(m => m.Stadium)
               .WithMany()
               .HasForeignKey("StadiumId")
               .OnDelete(DeleteBehavior.Restrict);

        // Incidencias: coleccion propiedad del partido (shadow FK "MatchId", Cascade: borrar el
        // partido borra sus incidencias). Acceso por backing field porque Incidents es de solo lectura.
        builder.HasMany(m => m.Incidents)
               .WithOne()
               .HasForeignKey("MatchId")
               .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(m => m.Incidents)
               .HasField("_incidents")
               .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
