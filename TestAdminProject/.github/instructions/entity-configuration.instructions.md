---
applyTo: "src/TestAdminProject.Infrastructure/EntityConfigurations/*.cs"
---

# 实体配置开发指南

## 开发原则

### 必须

- **配置定义**：
    - 必须实现 `IEntityTypeConfiguration<T>` 接口。
    - 每个实体一个配置文件。
- **字段配置**：
    - 必须配置主键，使用 `HasKey(x => x.Id)`。
    - 字符串属性必须设置最大长度。
    - 必填属性使用 `IsRequired()`。
    - 所有字段都不允许为 null，使用 `IsRequired()`。
    - 所有字段都必须提供注释说明。
    - 根据查询需求添加索引。
- **强类型ID配置**：
    - 对于强类型 ID，直接使用值生成器。
    - `IInt64StronglyTypedId` 使用 `UseSnowFlakeValueGenerator()`。
    - `IGuidStronglyTypedId` 使用 `UseGuidVersion7ValueGenerator()`。

### 必须不要

- **转换器**：不要使用 `HasConversion<{Id}.EfCoreValueConverter>()`，框架会自动处理强类型 ID 的转换。
- **RowVersion**：`RowVersion` 无需配置。

## 文件命名规则

- 类文件应放置在 `src/TestAdminProject.Infrastructure/EntityConfigurations/` 目录下。
- 文件名格式为 `{EntityName}EntityTypeConfiguration.cs`。

## 代码示例

**文件**: `src/TestAdminProject.Infrastructure/EntityConfigurations/UserEntityTypeConfiguration.cs`

```csharp
using TestAdminProject.Domain.AggregatesModel.UserAggregate;

namespace TestAdminProject.Infrastructure.EntityConfigurations;

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
