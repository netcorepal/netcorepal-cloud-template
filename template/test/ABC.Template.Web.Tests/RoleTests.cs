//#if (UseAdmin)
using Microsoft.EntityFrameworkCore;
using ABC.Template.Domain.AggregatesModel.RoleAggregate;
using ABC.Template.Infrastructure;
using ABC.Template.Web.Application.Queries;
using ABC.Template.Web.AppPermissions;
using ABC.Template.Web.Endpoints.Identity.Admin.RoleEndpoints;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Tests;

[Collection(WebAppTestCollection.Name)]
public class RoleTests(WebAppFixture app) : AuthenticatedTestBase<WebAppFixture>(app)
{
    /// <summary>
    /// 清理测试数据
    /// </summary>
    private async Task CleanupTestDataAsync()
    {
        using var scope = Fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // 删除所有测试角色（通过名称前缀识别）
        var testRoles = await dbContext.Roles
            .Where(r => r.Name.StartsWith("测试角色") || r.Name.StartsWith("TestRole"))
            .ToListAsync(TestContext.Current.CancellationToken);
        
        foreach (var role in testRoles)
        {
            role.SoftDelete();
        }
        
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    #region CreateRoleEndpoint Tests

    [Fact]
    public async Task CreateRole_WithValidData_ShouldSucceed()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var roleName = $"测试角色_{Guid.NewGuid():N}";
        var description = "测试角色描述";
        var permissionCodes = new[] { PermissionCodes.UserView, PermissionCodes.UserEdit };
        
        try
        {
            // Act
            var request = new CreateRoleRequest(roleName, description, permissionCodes);
            var (response, result) = await client.POSTAsync<CreateRoleEndpoint, CreateRoleRequest, ResponseData<CreateRoleResponse>>(request);
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(roleName, result.Data.Name);
            Assert.Equal(description, result.Data.Description);
            Assert.NotEqual(default(RoleId), result.Data.RoleId);
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    [Fact]
    public async Task CreateRole_WithDuplicateName_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var roleName = $"测试角色_{Guid.NewGuid():N}";
        var permissionCodes = new[] { PermissionCodes.UserView };
        
        try
        {
            // 先创建一个角色
            var request1 = new CreateRoleRequest(roleName, "描述1", permissionCodes);
            await client.POSTAsync<CreateRoleEndpoint, CreateRoleRequest, ResponseData<CreateRoleResponse>>(request1);
            
            // Act - 尝试创建同名角色
            var request2 = new CreateRoleRequest(roleName, "描述2", permissionCodes);
            var (response, result) = await client.POSTAsync<CreateRoleEndpoint, CreateRoleRequest, ResponseData<CreateRoleResponse>>(request2);
            
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
    public async Task CreateRole_WithEmptyName_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var permissionCodes = new[] { PermissionCodes.UserView };
        
        // Act
        var request = new CreateRoleRequest("", "描述", permissionCodes);
        var (response, result) = await client.POSTAsync<CreateRoleEndpoint, CreateRoleRequest, ResponseData<CreateRoleResponse>>(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    #endregion

    #region GetRoleEndpoint Tests

    [Fact]
    public async Task GetRole_WithValidId_ShouldSucceed()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var roleName = $"测试角色_{Guid.NewGuid():N}";
        var permissionCodes = new[] { PermissionCodes.UserView, PermissionCodes.UserEdit };
        RoleId roleId;
        
        try
        {
            // 创建测试角色
            var createRequest = new CreateRoleRequest(roleName, "测试描述", permissionCodes);
            var (_, createResult) = await client.POSTAsync<CreateRoleEndpoint, CreateRoleRequest, ResponseData<CreateRoleResponse>>(createRequest);
            roleId = createResult!.Data!.RoleId;
            
            // Act
            var request = new GetRoleRequest(roleId);
            var (response, result) = await client.GETAsync<GetRoleEndpoint, GetRoleRequest, ResponseData<RoleQueryDto?>>(request);
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(roleId, result.Data.RoleId);
            Assert.Equal(roleName, result.Data.Name);
            Assert.True(result.Data.PermissionCodes.Any());
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    [Fact]
    public async Task GetRole_WithNonExistentId_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var nonExistentId = new RoleId(Guid.NewGuid());
        
        // Act
        var request = new GetRoleRequest(nonExistentId);
        var (response, result) = await client.GETAsync<GetRoleEndpoint, GetRoleRequest, ResponseData<RoleQueryDto>>(request);
        
        // Assert
        Assert.False(response.IsSuccessStatusCode);
    }

    #endregion

    #region UpdateRoleEndpoint Tests

    [Fact]
    public async Task UpdateRole_WithValidData_ShouldSucceed()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var roleName = $"测试角色_{Guid.NewGuid():N}";
        var updatedName = $"更新后的角色_{Guid.NewGuid():N}";
        var permissionCodes = new[] { PermissionCodes.UserView };
        var updatedPermissionCodes = new[] { PermissionCodes.UserView, PermissionCodes.UserEdit, PermissionCodes.UserDelete };
        RoleId roleId;
        
        try
        {
            // 创建测试角色
            var createRequest = new CreateRoleRequest(roleName, "原始描述", permissionCodes);
            var (_, createResult) = await client.POSTAsync<CreateRoleEndpoint, CreateRoleRequest, ResponseData<CreateRoleResponse>>(createRequest);
            roleId = createResult!.Data!.RoleId;
            
            // Act
            var request = new UpdateRoleInfoRequest(roleId, updatedName, "更新后的描述", updatedPermissionCodes);
            var (response, result) = await client.PUTAsync<UpdateRoleEndpoint, UpdateRoleInfoRequest, ResponseData<bool>>(request);
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            
            // 验证更新成功
            var getRequest = new GetRoleRequest(roleId);
            var (getResponse, getResult) = await client.GETAsync<GetRoleEndpoint, GetRoleRequest, ResponseData<RoleQueryDto?>>(getRequest);
            Assert.True(getResponse.IsSuccessStatusCode);
            Assert.NotNull(getResult?.Data);
            Assert.Equal(roleId, getResult.Data.RoleId);
            Assert.Equal(updatedName, getResult.Data.Name);
            Assert.Equal("更新后的描述", getResult.Data.Description);
            Assert.Equal(3, getResult.Data.PermissionCodes.Count());
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    [Fact]
    public async Task UpdateRole_WithNonExistentId_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var nonExistentId = new RoleId(Guid.NewGuid());
        var permissionCodes = new[] { PermissionCodes.UserView };
        
        // Act
        var request = new UpdateRoleInfoRequest(nonExistentId, "新名称", "新描述", permissionCodes);
        var (response, result) = await client.PUTAsync<UpdateRoleEndpoint, UpdateRoleInfoRequest, ResponseData<bool>>(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    #endregion

    #region DeleteRoleEndpoint Tests

    [Fact]
    public async Task DeleteRole_ShouldSucceed()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var roleName = $"测试角色_{Guid.NewGuid():N}";
        var permissionCodes = new[] { PermissionCodes.UserView };
        RoleId roleId;
        
        try
        {
            // 创建测试角色
            var createRequest = new CreateRoleRequest(roleName, "测试描述", permissionCodes);
            var (_, createResult) = await client.POSTAsync<CreateRoleEndpoint, CreateRoleRequest, ResponseData<CreateRoleResponse>>(createRequest);
            roleId = createResult!.Data!.RoleId;
            
            // Act
            var response = await client.DeleteAsync($"/api/admin/roles/{roleId}", TestContext.Current.CancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseData<bool>>(responseContent);
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            
            // 验证角色已被软删除
            var getRequest = new GetRoleRequest(roleId);
            var (getResponse, _) = await client.GETAsync<GetRoleEndpoint, GetRoleRequest, ResponseData<RoleQueryDto?>>(getRequest);
            Assert.False(getResponse.IsSuccessStatusCode); // 软删除后查询不到
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    [Fact]
    public async Task DeleteRole_WithNonExistentId_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var nonExistentId = new RoleId(Guid.NewGuid());
        
        // Act
        var response = await client.DeleteAsync($"/api/admin/roles/{nonExistentId}", TestContext.Current.CancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseData<bool>>(responseContent);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    #endregion

    #region ActivateRoleEndpoint Tests

    [Fact]
    public async Task ActivateRole_ShouldSucceed()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var roleName = $"测试角色_{Guid.NewGuid():N}";
        var permissionCodes = new[] { PermissionCodes.UserView };
        RoleId roleId;
        
        try
        {
            // 创建测试角色
            var createRequest = new CreateRoleRequest(roleName, "测试描述", permissionCodes);
            var (_, createResult) = await client.POSTAsync<CreateRoleEndpoint, CreateRoleRequest, ResponseData<CreateRoleResponse>>(createRequest);
            roleId = createResult!.Data!.RoleId;
            
            // 先停用角色
            var deactivateRequest = new DeactivateRoleRequest(roleId);
            await client.PUTAsync<DeactivateRoleEndpoint, DeactivateRoleRequest, ResponseData<bool>>(deactivateRequest);
            
            // Act - 激活角色
            var request = new ActivateRoleRequest(roleId);
            var (response, result) = await client.PUTAsync<ActivateRoleEndpoint, ActivateRoleRequest, ResponseData<bool>>(request);
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            
            // 验证角色已激活
            var getRequest = new GetRoleRequest(roleId);
            var (getResponse, getResult) = await client.GETAsync<GetRoleEndpoint, GetRoleRequest, ResponseData<RoleQueryDto?>>(getRequest);
            Assert.True(getResponse.IsSuccessStatusCode);
            Assert.NotNull(getResult?.Data);
            Assert.True(getResult.Data.IsActive);
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    [Fact]
    public async Task ActivateRole_WhenAlreadyActive_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var roleName = $"测试角色_{Guid.NewGuid():N}";
        var permissionCodes = new[] { PermissionCodes.UserView };
        RoleId roleId;
        
        try
        {
            // 创建测试角色（默认激活）
            var createRequest = new CreateRoleRequest(roleName, "测试描述", permissionCodes);
            var (_, createResult) = await client.POSTAsync<CreateRoleEndpoint, CreateRoleRequest, ResponseData<CreateRoleResponse>>(createRequest);
            roleId = createResult!.Data!.RoleId;
            
            // Act - 尝试激活已激活的角色
            var request = new ActivateRoleRequest(roleId);
            var (response, result) = await client.PUTAsync<ActivateRoleEndpoint, ActivateRoleRequest, ResponseData<bool>>(request);
            
            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    #endregion

    #region DeactivateRoleEndpoint Tests

    [Fact]
    public async Task DeactivateRole_ShouldSucceed()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var roleName = $"测试角色_{Guid.NewGuid():N}";
        var permissionCodes = new[] { PermissionCodes.UserView };
        RoleId roleId;
        
        try
        {
            // 创建测试角色
            var createRequest = new CreateRoleRequest(roleName, "测试描述", permissionCodes);
            var (_, createResult) = await client.POSTAsync<CreateRoleEndpoint, CreateRoleRequest, ResponseData<CreateRoleResponse>>(createRequest);
            roleId = createResult!.Data!.RoleId;
            
            // Act - 停用角色
            var request = new DeactivateRoleRequest(roleId);
            var (response, result) = await client.PUTAsync<DeactivateRoleEndpoint, DeactivateRoleRequest, ResponseData<bool>>(request);
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            
            // 验证角色已停用
            var getRequest = new GetRoleRequest(roleId);
            var (getResponse, getResult) = await client.GETAsync<GetRoleEndpoint, GetRoleRequest, ResponseData<RoleQueryDto?>>(getRequest);
            Assert.True(getResponse.IsSuccessStatusCode);
            Assert.NotNull(getResult?.Data);
            Assert.False(getResult.Data.IsActive);
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    [Fact]
    public async Task DeactivateRole_WhenAlreadyInactive_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var roleName = $"测试角色_{Guid.NewGuid():N}";
        var permissionCodes = new[] { PermissionCodes.UserView };
        RoleId roleId;
        
        try
        {
            // 创建测试角色
            var createRequest = new CreateRoleRequest(roleName, "测试描述", permissionCodes);
            var (_, createResult) = await client.POSTAsync<CreateRoleEndpoint, CreateRoleRequest, ResponseData<CreateRoleResponse>>(createRequest);
            roleId = createResult!.Data!.RoleId;
            
            // 先停用角色
            var deactivateRequest1 = new DeactivateRoleRequest(roleId);
            await client.PUTAsync<DeactivateRoleEndpoint, DeactivateRoleRequest, ResponseData<bool>>(deactivateRequest1);
            
            // Act - 尝试停用已停用的角色
            var request = new DeactivateRoleRequest(roleId);
            var (response, result) = await client.PUTAsync<DeactivateRoleEndpoint, DeactivateRoleRequest, ResponseData<bool>>(request);
            
            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    #endregion
}
//#endif