---
applyTo: "src/ABC.Template.Infrastructure/ApplicationDbContext.cs"
---

# DbContext 添加聚合指南

## 开发原则

### 必须

- **命名空间**：在头部添加聚合根的命名空间。
- **DbSet 定义**：
    - 添加新聚合时在 DbSet 区域添加对应属性。
    - 使用 `=> Set<T>()` 模式定义 DbSet。
- **配置注册**：默认使用 `ApplyConfigurationsFromAssembly` 自动注册实体配置。

### 必须不要

- **额外配置**：无需手动注册实体配置，框架会自动扫描。

## 文件命名规则

- 文件在 `src/ABC.Template.Infrastructure/ApplicationDbContext.cs`。

## 代码示例

**文件**: `src/ABC.Template.Infrastructure/ApplicationDbContext.cs`

```csharp
public partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMediator mediator)
    : AppDbContextBase(options, mediator)
{
    // 现有的 DbSet
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<DeliverRecord> DeliverRecords => Set<DeliverRecord>();
    
    // 添加新聚合的 DbSet
    public DbSet<Customer> Customers => Set<Customer>();
}
```