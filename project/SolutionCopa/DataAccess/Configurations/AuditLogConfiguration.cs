using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCopa;

namespace DataAccess.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    private const int ActionMaxLength = 100;
    private const int DetailsMaxLength = 1000;
    private const int AffectedEntityTypeMaxLength = 100;

    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedOnAdd();              // Id IDENTITY (lo asigna la BD)
        builder.Property(l => l.Timestamp).IsRequired();
        builder.Property(l => l.Action).IsRequired().HasMaxLength(ActionMaxLength);
        builder.Property(l => l.Details).IsRequired().HasMaxLength(DetailsMaxLength);
        builder.Property(l => l.TournamentSettingsId).IsRequired();     // nullable recien en la Fase 7 del plan
        builder.Property(l => l.AffectedEntityType).HasMaxLength(AffectedEntityTypeMaxLength);  // opcional
        builder.Property(l => l.AffectedEntityId);                      // opcional
    }
}
