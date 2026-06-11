using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCopa;

namespace DataAccess.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    private const int ActionMaxLength = 100;
    private const int DetailsMaxLength = 1000;
    private const int AffectedEntityTypeMaxLength = 100;
    private const int PerformedByEmailMaxLength = 254;

    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedOnAdd();              // Id IDENTITY (lo asigna la BD)
        builder.Property(l => l.Timestamp).IsRequired();
        builder.Property(l => l.Action).IsRequired().HasMaxLength(ActionMaxLength);
        builder.Property(l => l.Details).IsRequired().HasMaxLength(DetailsMaxLength);
        builder.Property(l => l.TournamentSettingsId).IsRequired(false); // contexto opcional: los logs son globales por defecto
        builder.Property(l => l.PerformedByEmail).IsRequired(false).HasMaxLength(PerformedByEmailMaxLength);
        builder.Property(l => l.AffectedEntityType).HasMaxLength(AffectedEntityTypeMaxLength);  // opcional
        builder.Property(l => l.AffectedEntityId);                      // opcional
    }
}
