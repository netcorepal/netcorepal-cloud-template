# DDD开发最佳实践

## 概述

本文档总结了基于NetCorePal Cloud Framework开发DDD应用时的核心最佳实践，帮助开发者避免常见错误，提高代码质量和开发效率。

## 通用原则

### 1. 异步优先
- **所有I/O操作都应使用异步方法**
- 包括数据库访问、外部API调用、文件操作等
- 提高应用程序的可扩展性和性能

### 2. 取消令牌传递
- **将CancellationToken传递给所有异步操作**
- 支持请求取消，提高资源利用效率
- 在命令和查询处理器中正确传递取消令牌

### 3. 框架信任
- **信任框架的自动化功能，避免手动干预**
- 让框架处理SaveChanges、依赖注入、事务管理等
- 专注于业务逻辑而非技术细节

### 4. 类型安全
- **使用强类型ID，避免原始类型混淆**
- 利用编译时类型检查防止ID类型错误
- 提高代码的可读性和维护性

## 分层架构最佳实践

### 数据访问职责分离

#### 仓储 vs 查询的设计原则
**核心原则**: 仓储方法仅用于命令处理器中需要获取聚合进行业务操作的场景

**仓储方法适用场景**:
- 需要获取聚合根进行业务操作
- 需要调用聚合根的业务方法
- 需要修改聚合状态
- 体现特定的业务查找意图

**查询适用场景**:
- 纯粹的数据展示
- 报表和统计
- 列表查询和搜索
- 跨聚合的数据组合

**错误示例**:
```csharp
// ❌ 错误：在查询中使用仓储
public class GetUserListQueryHandler(IUserRepository userRepository)
{
    public async Task<UserListDto> Handle(GetUserListQuery request, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetPagedAsync(skip, pageSize, cancellationToken);
        // ...
    }
}
```

**正确示例**:
```csharp
// ✅ 正确：查询直接使用DbContext
public class GetUserListQueryHandler(ApplicationDbContext context)
{
    public async Task<UserListDto> Handle(GetUserListQuery request, CancellationToken cancellationToken)
    {
        var users = await context.Users
            .Skip(skip)
            .Take(pageSize)
            .Select(u => new UserListItemDto(u.Id, u.Name, u.Email))
            .ToListAsync(cancellationToken);
        // ...
    }
}

// ✅ 正确：命令使用仓储进行业务操作
public class UpdateUserCommandHandler(IUserRepository userRepository)
{
    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(request.UserId, cancellationToken);
        user.UpdateProfile(request.Name, request.Email); // 业务操作
    }
}
```

### Domain层
- **聚合根**: 不手动设置ID，由值生成器生成
- **业务规则**: 所有业务逻辑都在聚合根中实现
- **领域事件**: 在状态变化时发布领域事件
- **异常处理**: 使用KnownException处理业务异常

### Infrastructure层
- **仓储实现**: 仅提供业务导向的方法，使用异步操作
- **实体配置**: 完整的EF Core映射配置
- **值生成器**: 配置强类型ID的自动生成

### Application层
- **命令处理**: 使用仓储获取聚合，不手动调用SaveChanges
- **查询处理**: 直接使用DbContext，只读操作，不修改状态
- **验证**: 为所有命令和查询提供验证器

### Presentation层
- **API端点**: 使用正确的FastEndpoints响应方法
- **DTO设计**: 直接使用强类型ID
- **响应包装**: 使用ResponseData包装响应

## 常见错误避免

### ❌ 避免的错误做法

#### 1. ID生成错误
```csharp
// 错误：在聚合根中手动生成ID
public User(string name, string email)
{
    Id = new UserId(Guid.NewGuid()); // ❌ 不应在Domain层手动生成
    Id = UserId.New(); // ❌ 此方法不存在
}
```

#### 2. 事务管理错误
```csharp
// 错误：在命令处理器中手动调用SaveChanges
public async Task<UserId> Handle(CreateUserCommand request, CancellationToken cancellationToken)
{
    var user = new User(request.Name, request.Email);
    await userRepository.AddAsync(user, cancellationToken);
    await userRepository.UnitOfWork.SaveChangesAsync(cancellationToken); // ❌ 不应手动调用
    return user.Id;
}
```

#### 3. 同步方法使用
```csharp
// 错误：使用同步方法
repository.Add(entity); // ❌ 应该使用异步方法
repository.Update(entity); // ❌ 应该使用异步方法
```

#### 4. FastEndpoints响应错误
```csharp
// 错误：错误的响应方法名
await SendOkAsync(response); // ❌ 方法名错误
```

### ✅ 正确的做法

#### 1. ID生成正确做法
```csharp
// 正确：让值生成器自动生成ID
public User(string name, string email)
{
    // 不设置ID，由EF Core值生成器生成
    Name = name;
    Email = email;
    this.AddDomainEvent(new UserCreatedDomainEvent(this));
}
```

#### 2. 事务管理正确做法
```csharp
// 正确：让框架自动调用SaveChanges
public async Task<UserId> Handle(CreateUserCommand request, CancellationToken cancellationToken)
{
    var user = new User(request.Name, request.Email);
    await userRepository.AddAsync(user, cancellationToken);
    // 框架会自动调用SaveChanges
    return user.Id;
}
```

#### 3. 异步方法使用
```csharp
// 正确：使用异步方法
await repository.AddAsync(entity, cancellationToken);
await repository.UpdateAsync(entity, cancellationToken);
await repository.GetAsync(id, cancellationToken);
```

#### 4. FastEndpoints响应正确做法
```csharp
// 正确：使用正确的响应方法
await Send.OkAsync(response.AsResponseData(), ct);
await Send.CreatedAsync(response.AsResponseData(), ct);
await Send.NoContentAsync(ct);
```

## 性能优化建议

### 1. 异步编程
- 使用ConfigureAwait(false)在非UI应用中
- 避免异步方法的同步调用
- 正确使用Task.WhenAll进行并行操作

### 2. 数据库访问
- 使用异步数据库操作
- 合理使用EF Core的Include和Select
- 实现分页查询避免大数据集

### 3. 内存管理
- 正确使用using语句处理资源
- 避免在循环中创建大量对象
- 使用对象池减少GC压力

## 测试最佳实践

### 1. 单元测试
- 测试业务逻辑而非框架功能
- 使用构造函数创建强类型ID用于测试
- 验证领域事件的正确发布

### 2. 集成测试
- 测试完整的请求响应流程
- 使用TestContainers进行数据库测试
- 验证事务边界和数据一致性

## 代码审查检查清单

### Domain层检查
- [ ] 聚合根不手动设置ID
- [ ] 所有业务规则在聚合根中实现
- [ ] 状态变化时发布领域事件
- [ ] 使用KnownException处理业务异常

### Application层检查
- [ ] 命令处理器使用异步仓储方法
- [ ] 不手动调用SaveChanges
- [ ] 正确传递CancellationToken
- [ ] 所有命令和查询都有验证器

### Infrastructure层检查
- [ ] 实体配置包含必要的using语句
- [ ] 强类型ID配置了值生成器
- [ ] 仓储实现支持异步操作

### Presentation层检查
- [ ] 使用正确的FastEndpoints响应方法
- [ ] DTO中直接使用强类型ID
- [ ] 响应数据使用ResponseData包装

遵循这些最佳实践将确保您的DDD应用具有良好的架构、高性能和易维护性。
