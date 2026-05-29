using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCopa;

namespace DataAccess.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    private const int NameMaxLength = 60;
    private const int ConfederationMaxLength = 20;

    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();              // Id asignado por la app, no IDENTITY
        builder.Property(t => t.Name).IsRequired().HasMaxLength(NameMaxLength);
        builder.HasIndex(t => t.Name).IsUnique();                       // Name unico (regla de negocio)
        builder.Property(t => t.FifaRanking).IsRequired();
        builder.Property(t => t.IsFictitious).IsRequired();
        builder.Property(t => t.Confederation)
               .HasConversion<string>()                                 // enum como texto (legible en .sql)
               .HasMaxLength(ConfederationMaxLength)
               .IsRequired();
    }
}
