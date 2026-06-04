using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCopa;

namespace DataAccess.Configurations;

public class TournamentSettingsConfiguration : IEntityTypeConfiguration<TournamentSettings>
{
    public void Configure(EntityTypeBuilder<TournamentSettings> builder)
    {
        builder.ToTable("TournamentSettings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();              // Id definido por appsettings.json (default 1), no IDENTITY
        builder.Property(s => s.StartDate).IsRequired();
        builder.Property(s => s.MaxMatchesPerDay).IsRequired();
        builder.Property(s => s.DaysBetweenJourneys).IsRequired();
        builder.Property(s => s.FixtureSeed).IsRequired();
        builder.Property(s => s.CrossingsSeed).IsRequired();
        builder.Property(s => s.SimulationSeed).IsRequired();
        builder.Property(s => s.CompletionSeed).IsRequired();
    }
}
