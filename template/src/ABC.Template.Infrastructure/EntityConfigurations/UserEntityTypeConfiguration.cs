﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ABC.Template.Domain.AggregatesModel.UserAggregate;

namespace ABC.Template.Infrastructure.EntityConfigurations;

internal class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("user");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).UseSnowFlakeValueGenerator();

        builder.Property(b => b.Name).HasMaxLength(50).IsRequired();
        builder.Property(b => b.Email).HasMaxLength(100).IsRequired();
        builder.Property(b => b.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(b => b.Phone).HasMaxLength(20);
        builder.Property(b => b.RealName).HasMaxLength(50);
        builder.Property(b => b.Gender).HasMaxLength(10);
        builder.Property(b => b.Age);
        builder.Property(b => b.BirthDate);
        builder.Property(b => b.IsActive);
        builder.Property(b => b.CreatedAt);
        builder.Property(b => b.LastLoginTime);
        builder.Property(b => b.UpdateTime);

        builder.HasIndex(b => b.Name);
        builder.HasIndex(b => b.Email);

        builder.HasMany(au => au.Roles)
            .WithOne()
            .HasForeignKey(aur => aur.UserId)
            .OnDelete(DeleteBehavior.ClientCascade);
        builder.Navigation(au => au.Roles).AutoInclude();

        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(au => au.Dept)
            .WithOne()
            .HasForeignKey<UserDept>(ud => ud.UserId)
            .OnDelete(DeleteBehavior.ClientCascade);
        builder.Navigation(au => au.Dept).AutoInclude();
    }
}

internal class UserDeptEntityTypeConfiguration : IEntityTypeConfiguration<UserDept>
{
    public void Configure(EntityTypeBuilder<UserDept> builder)
    {
        builder.ToTable("user_dept");

        builder.HasKey(ud => ud.UserId);

        //#if (UseMongoDB)
        // MongoDB 要求主键必须映射到 _id 元素
        builder.Property(ud => ud.UserId)
            .HasElementName("_id");
        builder.Property(ud => ud.DeptId);
        builder.Property(ud => ud.DeptName).HasMaxLength(100);
        builder.Property(ud => ud.AssignedAt)
            .IsRequired();

        // 注意：不能为主键字段（映射到 _id）创建索引，因为 MongoDB 的 _id 字段已经有默认的唯一索引
        builder.HasIndex(ud => ud.DeptId);
        //#else
        builder.Property(ud => ud.UserId);
        builder.Property(ud => ud.DeptId);
        builder.Property(ud => ud.DeptName).HasMaxLength(100);
        builder.Property(ud => ud.AssignedAt)
            .IsRequired();

        builder.HasIndex(ud => ud.UserId);
        builder.HasIndex(ud => ud.DeptId);
        //#endif
    }
}

internal class UserRoleEntityTypeConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_role");

        builder.HasKey(t => new { t.UserId, t.RoleId });

        builder.Property(b => b.UserId);
        builder.Property(b => b.RoleId);
        builder.Property(b => b.RoleName).HasMaxLength(50).IsRequired();

        builder.HasOne<User>()
            .WithMany(u => u.Roles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
{
    public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
    {
        builder.ToTable("user_refresh_token");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseSnowFlakeValueGenerator();
        builder.Property(x => x.Token).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CreatedTime).IsRequired();
        builder.Property(x => x.ExpiresTime).IsRequired();
    }
}
