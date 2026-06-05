using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCopa;

namespace DataAccess.Configurations;

public class MatchEliminationConfiguration : IEntityTypeConfiguration<MatchElimination>
{
    private const int MatchCodeMaxLength = 20;
    private const int PhaseMaxLength = 20;

    public void Configure(EntityTypeBuilder<MatchElimination> builder)
    {
        builder.Property(m => m.MatchCode).HasColumnName("MatchCode").HasMaxLength(MatchCodeMaxLength);

        builder.Property(m => m.Phase)
               .HasConversion<string>()                                 // enum como texto (consistente con Confederation/Role)
               .HasMaxLength(PhaseMaxLength)
               .IsRequired();

        builder.Property(m => m.WonByPenalties).IsRequired();
        builder.Property(m => m.HomePenaltyGoals).IsRequired();
        builder.Property(m => m.AwayPenaltyGoals).IsRequired();

        // Navegaciones autorreferentes hacia los partidos de origen (cadena del bracket:
        // Final -> Semis -> ...). Backing fields para no disparar los setters con validacion.
        // FK opcional con ClientSetNull: NUNCA cascade, SQL Server rechaza ciclos de borrado
        // autorreferentes; al limpiar, EF nullea los FK antes de borrar.
        builder.HasOne(m => m.MatchHome)
               .WithMany()
               .HasForeignKey("MatchHomeId")
               .OnDelete(DeleteBehavior.ClientSetNull);
        builder.Navigation(m => m.MatchHome)
               .HasField("_matchHome")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(m => m.MatchAway)
               .WithMany()
               .HasForeignKey("MatchAwayId")
               .OnDelete(DeleteBehavior.ClientSetNull);
        builder.Navigation(m => m.MatchAway)
               .HasField("_matchAway")
               .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
