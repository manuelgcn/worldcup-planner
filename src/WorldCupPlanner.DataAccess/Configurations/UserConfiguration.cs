using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldCupPlanner.Domain;

namespace WorldCupPlanner.DataAccess.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    private const int EmailMaxLength = 254;
    private const int NameMaxLength = 100;
    private const int LastnameMaxLength = 100;
    private const int PasswordMaxLength = 100;

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.idUser);
        builder.Property(u => u.idUser).ValueGeneratedOnAdd();          // idUser IDENTITY (lo asigna la BD)
        builder.Property(u => u.email).IsRequired().HasMaxLength(EmailMaxLength);
        builder.HasIndex(u => u.email).IsUnique();                      // email unico (regla de negocio)
        builder.Property(u => u.name).IsRequired().HasMaxLength(NameMaxLength);
        builder.Property(u => u.lastname).HasMaxLength(LastnameMaxLength);
        builder.Property(u => u.password).IsRequired().HasMaxLength(PasswordMaxLength);
        builder.Property(u => u.dateOfBirth);                           // opcional
    }
}
