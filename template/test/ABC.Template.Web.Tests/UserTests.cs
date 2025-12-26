using System.Net;
using System.Net.Http.Headers;
using ABC.Template.Web.Endpoints.UserEndpoints;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Tests;

[Collection(WebAppTestCollection.Name)]
//#if (UseAspire)
public class UserTests(AspireHostAppFixture app)
//#else
public class UserTests(WebAppFixture app) : TestBase<WebAppFixture>
//#endif
{
    [Fact]
    public async Task Login_And_Auth_Test()
    {
        string userName = "testname";
        string password = "testpassword";
        var loginRequest = new LoginRequest(userName, password);
        var (rsp, res) = await app.Client.POSTAsync<LoginEndpoint, LoginRequest, ResponseData<string>>(loginRequest);
        
        Assert.True(rsp.IsSuccessStatusCode);
        Assert.NotNull(res);
        Assert.NotNull(res.Data);

        var jwtResponse1 = await app.Client.GETAsync<AuthEndpoint, ResponseData<bool>>();
        Assert.False(jwtResponse1.Response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, jwtResponse1.Response.StatusCode);

        app.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", res.Data);
        var jwtResponse2 = await app.Client.GETAsync<AuthEndpoint, ResponseData<bool>>();
        Assert.True(jwtResponse2.Response.IsSuccessStatusCode);
    }
}