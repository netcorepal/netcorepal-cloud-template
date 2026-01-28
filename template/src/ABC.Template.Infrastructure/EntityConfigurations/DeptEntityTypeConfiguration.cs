using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ABC.Template.Domain.AggregatesModel.DeptAggregate;

namespace ABC.Template.Infrastructure.EntityConfigurations;

/// <summary>
/// 部门实体类型配置
/// </summary>
internal class DeptEntityTypeConfiguration : IEntityTypeConfiguration<Dept>
{
    public void Configure(EntityTypeBuilder<Dept> builder)
    {
        builder.ToTable("dept");

        builder.HasKey(d => d.Id);
        builder.Property(t => t.Id).UseSnowFlakeValueGenerator();

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Remark)
            .HasMaxLength(500);

        builder.Property(d => d.Status)
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.IsDeleted)
            .IsRequired();

        builder.Property(d => d.DeletedAt);

        builder.Property(d => d.UpdateTime);

        // 索引
        builder.HasIndex(d => d.ParentId);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.IsDeleted);

        // 软删除过滤器
        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}