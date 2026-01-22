using Microsoft.EntityFrameworkCore;
using ABC.Template.Domain.AggregatesModel.DeptAggregate;
using ABC.Template.Domain.AggregatesModel.RoleAggregate;
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Infrastructure;
using ABC.Template.Web.Application.Queries;
using ABC.Template.Web.Endpoints.Identity.Admin.UserEndpoints;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Tests;

[Collection(WebAppTestCollection.Name)]
public class UserTests(WebAppFixture app) : AuthenticatedTestBase<WebAppFixture>(app)
{
    /// <summary>
    /// 获取种子数据中的角色ID（管理员角色）
    /// </summary>
    private async Task<RoleId> GetAdminRoleIdAsync()
    {
        using var scope = Fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "管理员", TestContext.Current.CancellationToken);
        return role?.Id ?? throw new InvalidOperationException("找不到管理员角色");
    }

    /// <summary>
    /// 获取种子数据中的部门ID（研发部门）
    /// </summary>
    private async Task<DeptId> GetDeptIdAsync()
    {
        using var scope = Fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var dept = await dbContext.Depts.FirstOrDefaultAsync(d => d.Name == "研发", TestContext.Current.CancellationToken);
        return dept?.Id ?? throw new InvalidOperationException("找不到研发部门");
    }

    /// <summary>
    /// 清理测试数据
    /// </summary>
    private async Task CleanupTestDataAsync()
    {
        using var scope = Fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // 删除所有测试用户（通过用户名前缀识别）
        var testUsers = await dbContext.Users
            .Where(u => u.Name.StartsWith("测试用户") || u.Name.StartsWith("TestUser"))
            .ToListAsync(TestContext.Current.CancellationToken);
        
        foreach (var user in testUsers)
        {
            user.SoftDelete();
        }
        
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    #region CreateUserEndpoint Tests

    [Fact]
    public async Task CreateUser_WithValidData_ShouldSucceed()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var userName = $"测试用户_{Guid.NewGuid():N}";
        var email = $"{userName}@test.com";
        var roleId = await GetAdminRoleIdAsync();
        var deptId = await GetDeptIdAsync();
        
        try
        {
            // Act
            var request = new CreateUserRequest(
                userName,
                email,
                "123456",
                "13800138000",
                "测试真实姓名",
                1,
                "男",
                DateTimeOffset.Now.AddYears(-25),
                deptId,
                "研发",
                new[] { roleId }
            );
            var (response, result) = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest, ResponseData<CreateUserResponse>>(request);
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(userName, result.Data.Name);
            Assert.Equal(email, result.Data.Email);
            Assert.NotEqual(default(UserId), result.Data.UserId);
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    [Fact]
    public async Task CreateUser_WithDuplicateName_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var userName = $"测试用户_{Guid.NewGuid():N}";
        var email = $"{userName}@test.com";
        var roleId = await GetAdminRoleIdAsync();
        var deptId = await GetDeptIdAsync();
        
        try
        {
            // 先创建一个用户
            var request1 = new CreateUserRequest(
                userName,
                email,
                "123456",
                "13800138000",
                "测试真实姓名",
                1,
                "男",
                DateTimeOffset.Now.AddYears(-25),
                deptId,
                "研发",
                new[] { roleId }
            );
            await client.POSTAsync<CreateUserEndpoint, CreateUserRequest, ResponseData<CreateUserResponse>>(request1);
            
            // Act - 尝试创建同名用户
            var request2 = new CreateUserRequest(
                userName,
                $"{userName}2@test.com",
                "123456",
                "13800138001",
                "测试真实姓名",
                1,
                "男",
                DateTimeOffset.Now.AddYears(-25),
                deptId,
                "研发",
                new[] { roleId }
            );
            var (response, result) = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest, ResponseData<CreateUserResponse>>(request2);
            
            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    [Fact]
    public async Task CreateUser_WithEmptyName_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var roleId = await GetAdminRoleIdAsync();
        var deptId = await GetDeptIdAsync();
        
        // Act
        var request = new CreateUserRequest(
            "",
            "test@test.com",
            "123456",
            "13800138000",
            "测试真实姓名",
            1,
            "男",
            DateTimeOffset.Now.AddYears(-25),
            deptId,
            "研发",
            new[] { roleId }
        );
        var (response, result) = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest, ResponseData<CreateUserResponse>>(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    #endregion

    #region GetUserEndpoint Tests

    [Fact]
    public async Task GetUser_WithValidId_ShouldSucceed()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var userName = $"测试用户_{Guid.NewGuid():N}";
        var email = $"{userName}@test.com";
        var roleId = await GetAdminRoleIdAsync();
        var deptId = await GetDeptIdAsync();
        UserId userId;
        
        try
        {
            // 创建测试用户
            var createRequest = new CreateUserRequest(
                userName,
                email,
                "123456",
                "13800138000",
                "测试真实姓名",
                1,
                "男",
                DateTimeOffset.Now.AddYears(-25),
                deptId,
                "研发",
                new[] { roleId }
            );
            var (createResponse, createResult) = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest, ResponseData<CreateUserResponse>>(createRequest);
            userId = createResult!.Data!.UserId;
            
            // Act
            var request = new GetUserRequest(userId);
            var (response, result) = await client.GETAsync<GetUserEndpoint, GetUserRequest, ResponseData<UserInfoQueryDto?>>(request);
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(userId, result.Data.UserId);
            Assert.Equal(userName, result.Data.Name);
            Assert.Equal(email, result.Data.Email);
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    [Fact]
    public async Task GetUser_WithNonExistentId_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var nonExistentId = new UserId(999999999);
        
        // Act
        var request = new GetUserRequest(nonExistentId);
        var (response, result) = await client.GETAsync<GetUserEndpoint, GetUserRequest, ResponseData<UserInfoQueryDto?>>(request);
        
        // Assert
        Assert.False(response.IsSuccessStatusCode);
    }

    #endregion

    #region UpdateUserEndpoint Tests

    [Fact]
    public async Task UpdateUser_WithValidData_ShouldSucceed()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var userName = $"测试用户_{Guid.NewGuid():N}";
        var email = $"{userName}@test.com";
        var updatedName = $"更新后的用户_{Guid.NewGuid():N}";
        var roleId = await GetAdminRoleIdAsync();
        var deptId = await GetDeptIdAsync();
        UserId userId;
        
        try
        {
            // 创建测试用户
            var createRequest = new CreateUserRequest(
                userName,
                email,
                "123456",
                "13800138000",
                "测试真实姓名",
                1,
                "男",
                DateTimeOffset.Now.AddYears(-25),
                deptId,
                "研发",
                new[] { roleId }
            );
            var (_, createResult) = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest, ResponseData<CreateUserResponse>>(createRequest);
            userId = createResult!.Data!.UserId;
            
            // Act
            var request = new UpdateUserRequest(
                userId,
                updatedName,
                $"{updatedName}@test.com",
                "13900139000",
                "更新后的真实姓名",
                1,
                "女",
                30,
                DateTimeOffset.Now.AddYears(-30),
                deptId,
                "研发",
                "" // 不更新密码
            );
            var (response, result) = await client.PUTAsync<UpdateUserEndpoint, UpdateUserRequest, ResponseData<UpdateUserResponse>>(request);
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(updatedName, result.Data.Name);
            
            // 验证更新成功
            var getRequest = new GetUserRequest(userId);
            var (getResponse, getResult) = await client.GETAsync<GetUserEndpoint, GetUserRequest, ResponseData<UserInfoQueryDto?>>(getRequest);
            Assert.True(getResponse.IsSuccessStatusCode);
            Assert.NotNull(getResult?.Data);
            Assert.Equal(updatedName, getResult.Data.Name);
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    [Fact]
    public async Task UpdateUser_WithNonExistentId_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var nonExistentId = new UserId(999999999);
        var deptId = await GetDeptIdAsync();
        
        // Act
        var request = new UpdateUserRequest(
            nonExistentId,
            "新名称",
            "new@test.com",
            "13800138000",
            "真实姓名",
            1,
            "男",
            25,
            DateTimeOffset.Now.AddYears(-25),
            deptId,
            "研发",
            ""
        );
        var (response, result) = await client.PUTAsync<UpdateUserEndpoint, UpdateUserRequest, ResponseData<UpdateUserResponse>>(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    #endregion

    #region DeleteUserEndpoint Tests

    [Fact]
    public async Task DeleteUser_ShouldSucceed()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var userName = $"测试用户_{Guid.NewGuid():N}";
        var email = $"{userName}@test.com";
        var roleId = await GetAdminRoleIdAsync();
        var deptId = await GetDeptIdAsync();
        UserId userId;
        
        try
        {
            // 创建测试用户
            var createRequest = new CreateUserRequest(
                userName,
                email,
                "123456",
                "13800138000",
                "测试真实姓名",
                1,
                "男",
                DateTimeOffset.Now.AddYears(-25),
                deptId,
                "研发",
                new[] { roleId }
            );
            var (_, createResult) = await client.POSTAsync<CreateUserEndpoint, CreateUserRequest, ResponseData<CreateUserResponse>>(createRequest);
            userId = createResult!.Data!.UserId;
            
            // Act
            var response = await client.DeleteAsync($"/api/admin/users/{userId}", TestContext.Current.CancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseData<bool>>(responseContent);
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            
            // 验证用户已被软删除
            var getRequest = new GetUserRequest(userId);
            var (getResponse, _) = await client.GETAsync<GetUserEndpoint, GetUserRequest, ResponseData<UserInfoQueryDto?>>(getRequest);
            Assert.False(getResponse.IsSuccessStatusCode); // 软删除后查询不到
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    [Fact]
    public async Task DeleteUser_WithNonExistentId_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var nonExistentId = new UserId(999999999);
        
        // Act
        var response = await client.DeleteAsync($"/api/admin/users/{nonExistentId}", TestContext.Current.CancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseData<bool>>(responseContent);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    #endregion
}