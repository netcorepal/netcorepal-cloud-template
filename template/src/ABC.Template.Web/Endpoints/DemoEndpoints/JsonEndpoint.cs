using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using NetCorePal.Extensions.Dto;
using NetCorePal.Extensions.Primitives;

namespace ABC.Template.Web.Endpoints.DemoEndpoints;

public partial record MyId : IGuidStronglyTypedId;

public record JsonRequest(MyId Id, string Name, DateTime Time);

public record JsonResponse(MyId Id, string Name, DateTime Time);

[Tags("Demo")]
[HttpPost("/demo/json")]
[AllowAnonymous]
public class JsonEndpoint : Endpoint<JsonRequest, ResponseData<JsonResponse>>
{
    public override Task HandleAsync(JsonRequest req, CancellationToken ct)
    {
        var response = new JsonResponse(req.Id, req.Name, req.Time);
        return Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}