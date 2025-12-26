using ABC.Template.Web.Endpoints.DemoEndpoints;
using NetCorePal.Context;
using NetCorePal.Extensions.Dto;
namespace ABC.Template.Web.Tests
{
    [Collection(WebAppTestCollection.Name)]
    public class DemoTests(WebAppFixture app) : TestBase<WebAppFixture>
    {
        [Fact]
        public async Task HealthCheckTest()
        {
            var response = await app.Client.GetAsync("/health", TestContext.Current.CancellationToken);
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task JsonTest()
        {
            var testGuid = Guid.NewGuid();
            var json = $$"""
                       {
                         "id": "{{testGuid}}",
                         "name": "myName",
                         "time": "2021-08-31T15:00:00"
                       }
                       """;
            var content = new StringContent(json);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await app.Client.PostAsync("/demo/json", content, TestContext.Current.CancellationToken);
            Assert.True(response.IsSuccessStatusCode);
            var responseData = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.NotNull(responseData);
            Assert.Contains("2021-08-31T15:00:00", responseData);
            Assert.Contains("\"name\":\"myName\"", responseData);
            Assert.Contains($"\"id\":\"{testGuid}\"", responseData);
        }

        [Fact]
        public async Task Json_ReadFromJson_Test()
        {
            var testGuid = Guid.NewGuid();
            var request = new JsonRequest(new MyId(testGuid), "myName", DateTime.Parse("2021-08-31 15:00:00"));
            var (rsp, res) = await app.Client.POSTAsync<JsonEndpoint, JsonRequest, ResponseData<JsonResponse>>(request);
            
            Assert.True(rsp.IsSuccessStatusCode);
            Assert.NotNull(res);
            Assert.Equal(DateTime.Parse("2021-08-31 15:00:00"), res.Data.Time);
            Assert.Equal("myName", res.Data.Name);
            Assert.Equal(testGuid, res.Data.Id.Id);
        }

        [Fact]
        public async Task ValidatorTest()
        {
            var request = new ValidatorRequest("", 0);
            var (rsp, res) = await app.Client.POSTAsync<ValidatorEndpoint, ValidatorRequest, ValidatorResponseData>(request);
            
            Assert.True(rsp.IsSuccessStatusCode);
            Assert.NotNull(res);
            Assert.False(res.success);
            Assert.Equal("不能为空", res.message);
            Assert.Equal(400, res.code);
            Assert.NotNull(res.errorData);
            Assert.Equal(2, res.errorData.Count());
            var errors = res.errorData.ToList();
            Assert.Equal("不能为空", errors[0].errorMessage);
            Assert.Equal("code1", errors[0].errorCode);
            Assert.Equal("name", errors[0].propertyName);
            Assert.Equal("价格必须在18-60之间", errors[1].errorMessage);
            Assert.Equal("code2", errors[1].errorCode);
            Assert.Equal("price", errors[1].propertyName);
        }


        record ValidatorResponseData(bool success, string message, int code, IEnumerable<ErrorData>? errorData);

        record ErrorData(string errorCode, string errorMessage, string propertyName);


        [Fact]
        public async Task ContextTest()
        {
            var client = app.CreateClient();
            client.DefaultRequestHeaders.Add(TenantContext.ContextKey, "t1");
            var (rsp, res) = await client.POSTAsync<ContextEndpoint, EmptyRequest, ResponseData<string>>(new EmptyRequest());
            
            Assert.True(rsp.IsSuccessStatusCode);
            Assert.NotNull(res);
            Assert.Equal("", res.Data);
        }

        [Fact]
        public async Task LockTest()
        {
            var task1 = app.Client.GETAsync<LockEndpoint, ResponseData<bool>>();
            var task2 = app.Client.GETAsync<LockEndpoint, ResponseData<bool>>();
            var results = await Task.WhenAll(task1, task2);
            
            Assert.True(results[0].Response.IsSuccessStatusCode);
            Assert.True(results[1].Response.IsSuccessStatusCode);
            Assert.NotNull(results[0].Result);
            Assert.NotNull(results[1].Result);
            Assert.False(results[0].Result.Data);
            Assert.False(results[1].Result.Data);
        }

        [Fact]
        public async Task CodeAnalysisTest()
        {
            var response = await app.Client.GetAsync("/code-analysis", TestContext.Current.CancellationToken);
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
            
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.NotNull(content);
            Assert.Contains("<!DOCTYPE html>", content);
        }
    }
}
