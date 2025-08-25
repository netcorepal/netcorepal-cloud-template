---
applyTo: "src/ABC.Template.Infrastructure/ApplicationDbContext.cs"
---

# DbContext 添加聚合指南

## 概述

ApplicationDbContext 是应用程序与数据库交互的核心组件，负责应用程序与数据库之间的所有操作。
当我们定义了新的聚合根时，需要在 DbContext 中添加相应的 DbSet 属性。

## 文件与目录

类文件命名应遵循以下规则：
- 文件在 `src/ABC.Template.Infrastructure/ApplicationDbContext.cs`

## 开发规则

在DbContext 中添加聚合根DbSet时应遵循以下规则：
- 在头部添加聚合根的命名空间
- 添加新聚合时在 DbSet 区域添加对应属性
- 使用 `=> Set<T>()` 模式定义 DbSet
- 默认使用 `ApplyConfigurationsFromAssembly` 自动注册实体配置，无需额外配置

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