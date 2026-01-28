using ABC.Template.Domain.AggregatesModel.DeptAggregate;
using ABC.Template.Domain.AggregatesModel.RoleAggregate;
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Infrastructure;
using ABC.Template.Web.AppPermissions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ABC.Template.Web.Utils;

/// <summary>
/// 数据库种子数据扩展方法
/// 用于在开发环境中初始化基础数据（角色、权限、组织架构、用户等）
/// </summary>
public static class SeedDatabaseExtension
{
    /// <summary>
    /// 初始化数据库种子数据
    /// 包括：角色和权限、组织架构、管理员用户、测试用户
    /// </summary>
    /// <param name="app">应用程序构建器</param>
    /// <returns>应用程序构建器</returns>
    internal static IApplicationBuilder SeedDatabase(this IApplicationBuilder app)
    {
        try
        {
            Log.Information("开始初始化数据库种子数据...");
            using var serviceScope = app.ApplicationServices.CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 初始化角色和权限
//#if (UseMongoDB)
            if (!dbContext.Roles.IgnoreAutoIncludes().Any())
//#else
            if (!dbContext.Roles.Any())
//#endif
        {
            var adminPermissionCodes = new[]
            {
                // 父权限码（用于菜单和路由权限控制）
                PermissionCodes.UserManagement,
                PermissionCodes.RoleManagement,
                PermissionCodes.DeptManagement,

                // 用户管理权限
                PermissionCodes.UserCreate,
                PermissionCodes.UserView,
                PermissionCodes.UserEdit,
                PermissionCodes.UserDelete,
                PermissionCodes.UserRoleAssign,
                PermissionCodes.UserResetPassword,

                // 角色管理权限
                PermissionCodes.RoleCreate,
                PermissionCodes.RoleView,
                PermissionCodes.RoleEdit,
                PermissionCodes.RoleDelete,
                PermissionCodes.RoleUpdatePermissions,

                // 部门管理权限
                PermissionCodes.DeptCreate,
                PermissionCodes.DeptView,
                PermissionCodes.DeptEdit,
                PermissionCodes.DeptDelete,

                // 所有接口访问权限
                PermissionCodes.AllApiAccess,
            };

            var adminPermissions = adminPermissionCodes.Select(code =>
            {
                var (name, description) = PermissionMapper.GetPermissionInfo(code);
                return new RolePermission(code, name, description);
            }).ToList();

            var userPermissionCodes = new[]
            {
                PermissionCodes.UserView,
                PermissionCodes.UserEdit,
                PermissionCodes.AllApiAccess,
            };

            var userPermissions = userPermissionCodes.Select(code =>
            {
                var (name, description) = PermissionMapper.GetPermissionInfo(code);
                // 对于 UserEdit，使用特殊描述
                if (code == PermissionCodes.UserEdit)
                {
                    description = "更新自己的用户信息";
                }
                return new RolePermission(code, name, description);
            }).ToList();

            var adminRole = new Role("管理员", "系统管理员", adminPermissions);
            var userRole = new Role("普通用户", "普通用户", userPermissions);

            dbContext.Roles.Add(adminRole);
            dbContext.Roles.Add(userRole);
            dbContext.SaveChanges();
        }

        // 初始化部门
        if (!dbContext.Depts.Any())
        {
            var dept = new Dept("研发", "根节点", new DeptId(0), 1);

            dbContext.Depts.Add(dept);
            dbContext.SaveChanges();

            var deptId = dbContext.Depts.FirstOrDefault(r => r.Name == "研发")?.Id;
            if (deptId == null)
            {
                throw new InvalidOperationException("无法找到部门'研发'，请确保部门已正确创建");
            }
            var childGroup = new Dept("Net", "第一个子节点", deptId, 1);
            dbContext.Depts.Add(childGroup);
            dbContext.SaveChanges();

            var childGroupId = dbContext.Depts.FirstOrDefault(r => r.Name == "Net")?.Id;
            if (childGroupId == null)
            {
                throw new InvalidOperationException("无法找到部门'Net'，请确保部门已正确创建");
            }
            var childIndividual = new Dept("C#", "第一个子节点的子节点", childGroupId, 1);
            dbContext.Depts.Add(childIndividual);
            dbContext.SaveChanges();
        }

        // 初始化管理员用户
//#if (UseMongoDB)
        // MongoDB 不支持对跨集合导航使用 Include，User 配置了 Roles/Dept 的 AutoInclude，此处用 IgnoreAutoIncludes 避免种子查询报错
        if (!dbContext.Users.IgnoreAutoIncludes().Any(u => u.Name == "admin"))
        {
            var dept = dbContext.Depts.FirstOrDefault(r => r.Name == "研发");
            var adminRole = dbContext.Roles.IgnoreAutoIncludes().FirstOrDefault(r => r.Name == "管理员");
//#else
        if (!dbContext.Users.Any(u => u.Name == "admin"))
        {
            var dept = dbContext.Depts.FirstOrDefault(r => r.Name == "研发");
            var adminRole = dbContext.Roles.FirstOrDefault(r => r.Name == "管理员");
//#endif
            
            if (dept == null)
            {
                throw new InvalidOperationException("无法找到部门'研发'，请确保部门已正确初始化");
            }
            
            if (adminRole == null)
            {
                throw new InvalidOperationException("无法找到角色'管理员'，请确保角色已正确初始化");
            }
            
            var adminUser = new User(
                "admin",
                "13800138000",
                PasswordHasher.HashPassword("123456"),
                new List<UserRole> { new UserRole(adminRole.Id, adminRole.Name) },
                "系统管理员",
                1,
                "admin@example.com",
                "男",
               DateTimeOffset.UtcNow.AddYears(-30) // 假设管理员年龄为30岁
            );

            // 设置部门关系
            adminUser.AssignDept(new UserDept(adminUser.Id, dept.Id, dept.Name));
            dbContext.Users.Add(adminUser);
            dbContext.SaveChanges();
        }

        // 初始化测试用户
//#if (UseMongoDB)
        if (!dbContext.Users.IgnoreAutoIncludes().Any(u => u.Name == "test"))
        {
            var dept = dbContext.Depts.FirstOrDefault(r => r.Name == "研发");
            var userRole = dbContext.Roles.IgnoreAutoIncludes().FirstOrDefault(r => r.Name == "普通用户");
//#else
        if (!dbContext.Users.Any(u => u.Name == "test"))
        {
            var dept = dbContext.Depts.FirstOrDefault(r => r.Name == "研发");
            var userRole = dbContext.Roles.FirstOrDefault(r => r.Name == "普通用户");
//#endif
            
            if (dept == null)
            {
                throw new InvalidOperationException("无法找到部门'研发'，请确保部门已正确初始化");
            }
            
            if (userRole == null)
            {
                throw new InvalidOperationException("无法找到角色'普通用户'，请确保角色已正确初始化");
            }
            
            var testUser = new User(
                "test",
                "13800138001",
                PasswordHasher.HashPassword("123456"),
                new List<UserRole> { new UserRole(userRole.Id, userRole.Name) },
                "测试用户",
                1,
                "test@example.com",
                "女",
               DateTimeOffset.UtcNow.AddYears(-25) // 假设测试用户年龄为25岁
            );

            testUser.AssignDept(new UserDept(testUser.Id, dept.Id, dept.Name));
            dbContext.Users.Add(testUser);
            dbContext.SaveChanges();
        }

            Log.Information("数据库种子数据初始化完成");
            return app;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "数据库种子数据初始化失败");
            throw;
        }
    }
}

