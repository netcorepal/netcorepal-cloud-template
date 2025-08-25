---
applyTo: "src/ABC.Template.Infrastructure/EntityConfigurations/*.cs"
---

# 实体配置开发指南

## 概述

实体配置负责定义聚合根和实体与数据库表的映射关系，使用 Entity Framework Core 的 Fluent API。配置文件位于基础设施层，确保领域层不依赖于数据访问技术。

## 文件与目录

类文件命名应遵循以下规则：
- 应放置在 `src/ABC.Template.Infrastructure/EntityConfigurations/` 目录下
- 文件名格式为 `{EntityName}EntityTypeConfiguration.cs`
- 每个实体一个配置文件
- 实现 `IEntityTypeConfiguration<T>` 接口

## 开发规则

实体配置的定义应遵循以下规则：
- 必须实现 `IEntityTypeConfiguration<T>` 接口
- 必须配置强类型ID转换器，使用 `HasConversion<{Id}.EfCoreValueConverter>()`
- 必须配置主键，使用 `HasKey(x => x.Id)`
- 字符串属性必须设置最大长度
- 必填属性使用 `IsRequired()`
- 根据查询需求添加索引
- 所有字段都不允许为null，使用 `IsRequired()`
- 所有字段都必须提供注释说明

强类型Id值生成器配置：
- IInt64StronglyTypedId 使用 UseSnowFlakeValueGenerator()
- IGuidStronglyTypedId 使用 UseGuidVersion7ValueGenerator()

## 代码示例

**文件**: `src/ABC.Template.Infrastructure/EntityConfigurations/UserEntityTypeConfiguration.cs`

```csharp
using ABC.Template.Domain.AggregatesModel.UserAggregate;

namespace ABC.Template.Infrastructure.EntityConfigurations;

public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .UseGuidVersion7ValueGenerator()
            .HasComment("用户标识");

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("用户姓名");
        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("用户邮箱");

        builder.HasIndex(x => x.Email)
            .IsUnique();
    }
}
```
