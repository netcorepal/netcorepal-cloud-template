using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Endpoints;

/// <summary>
/// 默认 Hello 接口，用于健康检查或简单连通性测试
/// </summary>
[Tags("Hello")]
[HttpGet("/api/hello")]
[AllowAnonymous]
public class HelloEndpoint : EndpointWithoutRequest<ResponseData<string>>
{
    public override Task HandleAsync(CancellationToken ct)
    {
        return Send.OkAsync("hello".AsResponseData(), cancellation: ct);
    }
}
