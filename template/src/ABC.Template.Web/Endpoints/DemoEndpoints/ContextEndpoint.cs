using FastEndpoints;
using NetCorePal.Context;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Endpoints.DemoEndpoints;

[Tags("Demo")]
[HttpPost("/demo/context")]
public class ContextEndpoint(IContextAccessor contextAccessor) : EndpointWithoutRequest<ResponseData<string>>
{
    public override Task HandleAsync(CancellationToken ct)
    {
        var tenantContext = contextAccessor.GetContext<TenantContext>();
        var result = (tenantContext == null ? "" : tenantContext.TenantId);
        return Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}