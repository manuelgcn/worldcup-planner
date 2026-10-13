using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Configurations;

// TPH (Table-per-Hierarchy) para la jerarquia MatchIncident: una unica tabla "MatchIncidents"
// con discriminador "IncidentType". OCP: agregar un nuevo tipo de incidencia solo requiere una
// linea HasValue<...> aqui, sin tocar Match ni el resto del esquema.
public class MatchIncidentConfiguration : IEntityTypeConfiguration<MatchIncident>
{
    public void Configure(EntityTypeBuilder<MatchIncident> builder)
    {
        builder.ToTable("MatchIncidents");
        builder.HasKey(incident => incident.Id);
        builder.Property(incident => incident.Id).ValueGeneratedNever();    // Id asignado por la app, no IDENTITY
        builder.Property(incident => incident.Minute).IsRequired();

        builder.HasDiscriminator<string>("IncidentType")
               .HasValue<YellowCardIncident>("YellowCard")
               .HasValue<RedCardIncident>("RedCard");

        // Team: shadow FK requerida con Restrict (no se borra un equipo referenciado por una incidencia).
        builder.HasOne(incident => incident.Team)
               .WithMany()
               .HasForeignKey("TeamId")
               .OnDelete(DeleteBehavior.Restrict);
    }
}
