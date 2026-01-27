using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ABC.Template.Domain.AggregatesModel.RoleAggregate;

namespace ABC.Template.Infrastructure.EntityConfigurations;

internal class RoleEntityTypeConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("role");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).UseGuidVersion7ValueGenerator();

        builder.Property(b => b.Name).HasMaxLength(50).IsRequired();
        builder.Property(b => b.Description).HasMaxLength(200);
        builder.Property(b => b.CreatedAt);
        builder.Property(b => b.IsActive);
        builder.Property(b => b.IsDeleted);

        builder.HasIndex(b => b.Name).IsUnique();

        builder.HasMany(r => r.Permissions).WithOne().HasForeignKey(rp => rp.RoleId);
        builder.Navigation(e => e.Permissions).AutoInclude();

        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}

internal class RolePermissionEntityTypeConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permission");

        builder.HasKey(t => new { t.RoleId, t.PermissionCode });

        builder.Property(b => b.RoleId);
        builder.Property(b => b.PermissionCode).HasMaxLength(100).IsRequired();
        builder.Property(b => b.PermissionName).HasMaxLength(100);
        builder.Property(b => b.PermissionDescription).HasMaxLength(200);

        builder.HasOne<Role>()
            .WithMany(r => r.Permissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}