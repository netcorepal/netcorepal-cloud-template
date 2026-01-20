//#if (UseAdmin)
using Microsoft.EntityFrameworkCore;
using ABC.Template.Domain.AggregatesModel.DeptAggregate;
using ABC.Template.Infrastructure;
using ABC.Template.Web.Application.Queries;
using ABC.Template.Web.Endpoints.Identity.Admin.DeptEndpoints;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Tests;

[Collection(WebAppTestCollection.Name)]
public class DeptTests(WebAppFixture app) : AuthenticatedTestBase<WebAppFixture>(app)
{

    /// <summary>
    /// 创建测试部门
    /// </summary>
    protected async Task<DeptId> CreateTestDeptAsync(HttpClient client, string name, string remark = "测试备注", DeptId? parentId = null, int status = 1)
    {
        var request = new CreateDeptRequest(name, remark, parentId, status);
        var (response, result) = await client.POSTAsync<CreateDeptEndpoint, CreateDeptRequest, ResponseData<CreateDeptResponse>>(request);
        
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        
        return result.Data.Id;
    }

    /// <summary>
    /// 清理测试数据
    /// </summary>
    protected async Task CleanupTestDataAsync()
    {
        using var scope = Fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // 删除所有测试部门（通过名称前缀识别）
        var testDepts = await dbContext.Depts
            .Where(d => d.Name.StartsWith("测试部门") || d.Name.StartsWith("TestDept"))
            .ToListAsync(TestContext.Current.CancellationToken);
        
        foreach (var dept in testDepts)
        {
            dept.SoftDelete();
        }
        
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    #region CreateDeptEndpoint Tests

    [Fact]
    public async Task CreateDept_WithValidData_ShouldSucceed()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var deptName = $"测试部门_{Guid.NewGuid():N}";
        
        try
        {
            // Act
            var request = new CreateDeptRequest(deptName, "测试备注", null, 1);
            var (response, result) = await client.POSTAsync<CreateDeptEndpoint, CreateDeptRequest, ResponseData<CreateDeptResponse>>(request);
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(deptName, result.Data.Name);
            Assert.Equal("测试备注", result.Data.Remark);
            Assert.NotEqual(default(DeptId), result.Data.Id);
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    [Fact]
    public async Task CreateDept_WithParentId_ShouldSucceed()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var parentDeptName = $"测试部门_父_{Guid.NewGuid():N}";
        var childDeptName = $"测试部门_子_{Guid.NewGuid():N}";
        
        try
        {
            // 先创建父部门
            var parentId = await CreateTestDeptAsync(client, parentDeptName);
            
            // Act - 创建子部门
            var request = new CreateDeptRequest(childDeptName, "子部门备注", parentId, 1);
            var (response, result) = await client.POSTAsync<CreateDeptEndpoint, CreateDeptRequest, ResponseData<CreateDeptResponse>>(request);
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(childDeptName, result.Data.Name);
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    [Fact]
    public async Task CreateDept_WithDuplicateName_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var deptName = $"测试部门_{Guid.NewGuid():N}";
        
        try
        {
            // 先创建一个部门
            await CreateTestDeptAsync(client, deptName);
            
            // Act - 尝试创建同名部门
            var request = new CreateDeptRequest(deptName, "重复名称测试", null, 1);
            var (response, result) = await client.POSTAsync<CreateDeptEndpoint, CreateDeptRequest, ResponseData<CreateDeptResponse>>(request);
            
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
    public async Task CreateDept_WithEmptyName_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        
        // Act
        var request = new CreateDeptRequest("", "备注", null, 1);
        var (response, result) = await client.POSTAsync<CreateDeptEndpoint, CreateDeptRequest, ResponseData<CreateDeptResponse>>(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateDept_WithInvalidStatus_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var deptName = $"测试部门_{Guid.NewGuid():N}";
        
        // Act
        var request = new CreateDeptRequest(deptName, "备注", null, 2); // 无效的状态值
        var (response, result) = await client.POSTAsync<CreateDeptEndpoint, CreateDeptRequest, ResponseData<CreateDeptResponse>>(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    #endregion

    #region GetDeptEndpoint Tests

    [Fact]
    public async Task GetDept_WithValidId_ShouldSucceed()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var deptName = $"测试部门_{Guid.NewGuid():N}";
        DeptId deptId;
        
        try
        {
            deptId = await CreateTestDeptAsync(client, deptName);
            
            // Act
            var request = new GetDeptRequest(deptId);
            var (response, result) = await client.GETAsync<GetDeptEndpoint, GetDeptRequest, ResponseData<GetDeptResponse?>>(request);
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(deptId, result.Data.Id);
            Assert.Equal(deptName, result.Data.Name);
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    [Fact]
    public async Task GetDept_WithNonExistentId_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var nonExistentId = new DeptId(999999999);
        
        // Act
        var request = new GetDeptRequest(nonExistentId);
        var (response, result) = await client.GETAsync<GetDeptEndpoint, GetDeptRequest, ResponseData<GetDeptResponse?>>(request);
        
        // Assert
        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task GetDept_WithInvalidIdFormat_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        
        // Act - 使用无效的ID格式（通过URL传递）
        var response = await client.GetAsync("/api/admin/dept/invalid-id", TestContext.Current.CancellationToken);
        
        // Assert - 对于路由错误，直接检查HTTP状态码
        Assert.False(response.IsSuccessStatusCode);
    }

    #endregion

    #region GetDeptTreeEndpoint Tests

    [Fact]
    public async Task GetDeptTree_ShouldReturnTreeStructure()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var parentDeptName = $"测试部门_父_{Guid.NewGuid():N}";
        var childDeptName = $"测试部门_子_{Guid.NewGuid():N}";
        
        try
        {
            // 创建父子部门
            var parentId = await CreateTestDeptAsync(client, parentDeptName);
            await CreateTestDeptAsync(client, childDeptName, "子部门", parentId, 1);
            
            // Act
            var request = new GetDeptTreeRequest(false);
            var (response, result) = await client.GETAsync<GetDeptTreeEndpoint, GetDeptTreeRequest, ResponseData<IEnumerable<DeptTreeDto>>>(request);
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            
            // 验证树结构包含创建的部门
            var allDepts = FlattenTree(result.Data);
            Assert.Contains(allDepts, d => d.Name == parentDeptName);
            Assert.Contains(allDepts, d => d.Name == childDeptName);
            
            // 验证父子关系
            var parent = allDepts.FirstOrDefault(d => d.Name == parentDeptName);
            Assert.NotNull(parent);
            var child = allDepts.FirstOrDefault(d => d.Name == childDeptName);
            Assert.NotNull(child);
            Assert.Equal(parent.Id, child.ParentId);
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    [Fact]
    public async Task GetDeptTree_WithIncludeInactive_ShouldIncludeInactiveDepts()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var activeDeptName = $"测试部门_激活_{Guid.NewGuid():N}";
        var inactiveDeptName = $"测试部门_停用_{Guid.NewGuid():N}";
        
        try
        {
            // 创建激活和停用的部门
            await CreateTestDeptAsync(client, activeDeptName, "激活部门", null, 1);
            var inactiveId = await CreateTestDeptAsync(client, inactiveDeptName, "停用部门", null, 0);
            
            // Act - 不包含非激活部门
            var requestExcludeInactive = new GetDeptTreeRequest(false);
            var (response1, result1) = await client.GETAsync<GetDeptTreeEndpoint, GetDeptTreeRequest, ResponseData<IEnumerable<DeptTreeDto>>>(requestExcludeInactive);
            
            // Act - 包含非激活部门
            var requestIncludeInactive = new GetDeptTreeRequest(true);
            var (response2, result2) = await client.GETAsync<GetDeptTreeEndpoint, GetDeptTreeRequest, ResponseData<IEnumerable<DeptTreeDto>>>(requestIncludeInactive);
            
            // Assert
            Assert.True(response1.IsSuccessStatusCode);
            Assert.True(response2.IsSuccessStatusCode);
            
            var treeExcludeInactive = FlattenTree(result1?.Data ?? Enumerable.Empty<DeptTreeDto>());
            var treeIncludeInactive = FlattenTree(result2?.Data ?? Enumerable.Empty<DeptTreeDto>());
            
            // 排除非激活时不应包含停用部门
            Assert.DoesNotContain(treeExcludeInactive, d => d.Name == inactiveDeptName);
            Assert.Contains(treeExcludeInactive, d => d.Name == activeDeptName);
            
            // 包含非激活时应包含停用部门
            Assert.Contains(treeIncludeInactive, d => d.Name == inactiveDeptName);
            Assert.Contains(treeIncludeInactive, d => d.Name == activeDeptName);
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    /// <summary>
    /// 展平树结构为列表
    /// </summary>
    private static List<DeptTreeDto> FlattenTree(IEnumerable<DeptTreeDto> tree)
    {
        var result = new List<DeptTreeDto>();
        foreach (var node in tree)
        {
            result.Add(node);
            if (node.Children.Any())
            {
                result.AddRange(FlattenTree(node.Children));
            }
        }
        return result;
    }

    #endregion

    #region UpdateDeptEndpoint Tests

    [Fact]
    public async Task UpdateDept_WithValidData_ShouldSucceed()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var deptName = $"测试部门_{Guid.NewGuid():N}";
        var updatedName = $"更新后的部门_{Guid.NewGuid():N}";
        
        try
        {
            var deptId = await CreateTestDeptAsync(client, deptName, "原始备注");
            
            // Act
            var request = new UpdateDeptRequest(deptId, updatedName, "更新后的备注", new DeptId(0), 1);
            var (response, result) = await client.PUTAsync<UpdateDeptEndpoint, UpdateDeptRequest, ResponseData<bool>>(request);
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            
            // 验证更新成功
            var getRequest = new GetDeptRequest(deptId);
            var (getResponse, getResult) = await client.GETAsync<GetDeptEndpoint, GetDeptRequest, ResponseData<GetDeptResponse?>>(getRequest);
            Assert.True(getResponse.IsSuccessStatusCode);
            Assert.NotNull(getResult?.Data);
            Assert.Equal(updatedName, getResult.Data.Name);
            Assert.Equal("更新后的备注", getResult.Data.Remark);
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    [Fact]
    public async Task UpdateDept_WithNonExistentId_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var nonExistentId = new DeptId(999999999);
        
        // Act
        var request = new UpdateDeptRequest(nonExistentId, "新名称", "新备注", new DeptId(0), 1);
            var (response, result) = await client.PUTAsync<UpdateDeptEndpoint, UpdateDeptRequest, ResponseData<bool>>(request);
            
            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
    }

    [Fact]
    public async Task UpdateDept_WithEmptyName_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var deptName = $"测试部门_{Guid.NewGuid():N}";
        
        try
        {
            var deptId = await CreateTestDeptAsync(client, deptName);
            
            // Act
            var request = new UpdateDeptRequest(deptId, "", "备注", new DeptId(0), 1);
            var (response, result) = await client.PUTAsync<UpdateDeptEndpoint, UpdateDeptRequest, ResponseData<bool>>(request);
            
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
    public async Task UpdateDept_WithInvalidStatus_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var deptName = $"测试部门_{Guid.NewGuid():N}";
        
        try
        {
            var deptId = await CreateTestDeptAsync(client, deptName);
            
            // Act
            var request = new UpdateDeptRequest(deptId, "新名称", "备注", new DeptId(0), 2); // 无效的状态值
            var (response, result) = await client.PUTAsync<UpdateDeptEndpoint, UpdateDeptRequest, ResponseData<bool>>(request);
            
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

    #region DeleteDeptEndpoint Tests

    [Fact]
    public async Task DeleteDept_WithoutChildren_ShouldSucceed()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var deptName = $"测试部门_{Guid.NewGuid():N}";
        
        try
        {
            var deptId = await CreateTestDeptAsync(client, deptName);
            
            // Act - 使用URL路径参数调用DELETE
            var response = await client.DeleteAsync($"/api/admin/dept/{deptId}", TestContext.Current.CancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseData<bool>>(responseContent);
            
            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            
            // 验证部门已被软删除
            var getRequest = new GetDeptRequest(deptId);
            var (getResponse, getResult) = await client.GETAsync<GetDeptEndpoint, GetDeptRequest, ResponseData<GetDeptResponse?>>(getRequest);
            Assert.False(getResponse.IsSuccessStatusCode); // 软删除后查询不到
        }
        finally
        {
            await CleanupTestDataAsync();
        }
    }

    [Fact]
    public async Task DeleteDept_WithChildren_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var parentDeptName = $"测试部门_父_{Guid.NewGuid():N}";
        var childDeptName = $"测试部门_子_{Guid.NewGuid():N}";
        
        try
        {
            // 创建父子部门
            var parentId = await CreateTestDeptAsync(client, parentDeptName);
            await CreateTestDeptAsync(client, childDeptName, "子部门", parentId, 1);
            
            // Act - 尝试删除有子部门的父部门
            var response = await client.DeleteAsync($"/api/admin/dept/{parentId}", TestContext.Current.CancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseData<bool>>(responseContent);
            
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
    public async Task DeleteDept_WithNonExistentId_ShouldFail()
    {
        // Arrange
        var client = await GetAuthenticatedClientAsync();
        var nonExistentId = new DeptId(999999999);
        
        // Act
        var response = await client.DeleteAsync($"/api/admin/dept/{nonExistentId}", TestContext.Current.CancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var result = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseData<bool>>(responseContent);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    #endregion
}
//#endif