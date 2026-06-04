using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectCopa;

namespace DataAccess.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    private const int RoleMaxLength = 20;

    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();
        builder.Property(r => r.Role)
               .HasConversion<string>()                                 // enum como texto (consistente con Confederation)
               .HasMaxLength(RoleMaxLength)
               .IsRequired();
        builder.HasOne(r => r.User)
               .WithMany(u => u.Roles)
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Cascade);                       // borrar el usuario borra sus roles
        builder.HasIndex(r => new { r.UserId, r.Role }).IsUnique();     // un rol no se repite por usuario
    }
}
