using ABC.Template.Domain.AggregatesModel.DeptAggregate;
using ABC.Template.Domain.AggregatesModel.RoleAggregate;
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Infrastructure;
using ABC.Template.Web.AppPermissions;

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
        using var serviceScope = app.ApplicationServices.CreateScope();
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // 初始化角色和权限
        if (!dbContext.Roles.Any())
        {
            var adminPermissions = new List<RolePermission>
            {
                // 父权限码（用于菜单和路由权限控制）
                new RolePermission(PermissionCodes.UserManagement, "用户管理", "用户管理"),
                new RolePermission(PermissionCodes.RoleManagement, "角色管理", "角色管理"),
                new RolePermission(PermissionCodes.DeptManagement, "部门管理", "部门管理"),

                // 用户管理权限
                new RolePermission(PermissionCodes.UserCreate, "创建用户", "创建新用户"),
                new RolePermission(PermissionCodes.UserView, "查看用户", "查看用户信息"),
                new RolePermission(PermissionCodes.UserEdit, "更新用户", "更新用户信息"),
                new RolePermission(PermissionCodes.UserDelete, "删除用户", "删除用户"),
                new RolePermission(PermissionCodes.UserRoleAssign, "分配用户角色", "分配用户角色权限"),
                new RolePermission(PermissionCodes.UserResetPassword, "重置用户密码", "重置用户密码"),

                // 角色管理权限
                new RolePermission(PermissionCodes.RoleCreate, "创建角色", "创建新角色"),
                new RolePermission(PermissionCodes.RoleView, "查看角色", "查看角色信息"),
                new RolePermission(PermissionCodes.RoleEdit, "更新角色", "更新角色信息"),
                new RolePermission(PermissionCodes.RoleDelete, "删除角色", "删除角色"),
                new RolePermission(PermissionCodes.RoleUpdatePermissions, "更新角色权限", "更新角色的权限"),

                // 部门管理权限
                new RolePermission(PermissionCodes.DeptCreate, "创建部门", "创建部门"),
                new RolePermission(PermissionCodes.DeptView, "查看部门", "查看部门信息"),
                new RolePermission(PermissionCodes.DeptEdit, "更新部门", "更新部门信息"),
                new RolePermission(PermissionCodes.DeptDelete, "删除部门", "删除部门"),

                // 所有接口访问权限
                new RolePermission(PermissionCodes.AllApiAccess, "所有接口访问权限", "所有接口访问权限"),
            };

            var userPermissions = new List<RolePermission>
            {
                new RolePermission(PermissionCodes.UserView, "查看用户", "查看用户信息"),
                new RolePermission(PermissionCodes.UserEdit, "更新用户", "更新自己的用户信息"),
                new RolePermission(PermissionCodes.AllApiAccess, "所有接口访问权限", "所有接口访问权限"),
            };

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
        if (!dbContext.Users.Any(u => u.Name == "admin"))
        {
            var dept = dbContext.Depts.FirstOrDefault(r => r.Name == "研发");
            var adminRole = dbContext.Roles.FirstOrDefault(r => r.Name == "管理员");
            
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
                DateTimeOffset.Now.AddYears(-30) // 假设管理员年龄为30岁
            );

            // 设置部门关系
            adminUser.AssignDept(new UserDept(adminUser.Id, dept.Id, dept.Name));
            dbContext.Users.Add(adminUser);
            dbContext.SaveChanges();
        }

        // 初始化测试用户
        if (!dbContext.Users.Any(u => u.Name == "test"))
        {
            var dept = dbContext.Depts.FirstOrDefault(r => r.Name == "研发");
            var userRole = dbContext.Roles.FirstOrDefault(r => r.Name == "普通用户");
            
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
                DateTimeOffset.Now.AddYears(-25) // 假设测试用户年龄为25岁
            );

            testUser.AssignDept(new UserDept(testUser.Id, dept.Id, dept.Name));
            dbContext.Users.Add(testUser);
            dbContext.SaveChanges();
        }

        return app;
    }
}

