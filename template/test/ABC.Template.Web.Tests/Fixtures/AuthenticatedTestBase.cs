using System.Net.Http.Headers;
using NetCorePal.Extensions.Dto;
using ABC.Template.Web.Endpoints.Identity.Admin.UserEndpoints;

namespace ABC.Template.Web.Tests.Fixtures;

/// <summary>
/// 提供认证功能的测试基类
/// </summary>
public abstract class AuthenticatedTestBase<TFixture>(TFixture fixture) : TestBase<TFixture> where TFixture : WebAppFixture
{
    /// <summary>
    /// 暴露Fixture属性供子类使用
    /// </summary>
    protected TFixture Fixture => fixture;

    /// <summary>
    /// 获取带认证的HttpClient
    /// </summary>
    protected async Task<HttpClient> GetAuthenticatedClientAsync()
    {
        var client = fixture.CreateClient();
        
        // 登录获取token
        var loginRequest = new LoginRequest("admin", "123456");
        var (_, loginResponse) = await client.POSTAsync<LoginEndpoint, LoginRequest, ResponseData<LoginResponse>>(loginRequest);
        
        if (loginResponse?.Data == null)
        {
            throw new InvalidOperationException("登录失败，无法获取token");
        }
        
        // 设置Authorization header
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Data.Token);
        
        return client;
    }
}