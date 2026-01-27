using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Endpoints;

/// <summary>
/// Hello
/// </summary>
public class HelloEndpoint : EndpointWithoutRequest<ResponseData<string>>
{
    public override void Configure()
    {
        Tags("Hello");
        Description(b => b.AutoTagOverride("Hello"));
        Get("/api/hello");
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        return Send.OkAsync("hello".AsResponseData(), cancellation: ct);
    }
}
