using FastEndpoints;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Endpoints.DemoEndpoints;

[Tags("Demo")]
[HttpPost("/demo/json")]
public class JsonEndpoint : Endpoint<JsonRequest, ResponseData<JsonResponse>>
{
    public override Task HandleAsync(JsonRequest req, CancellationToken ct)
    {
        var response = new JsonResponse(req.Id, req.Name, req.Time);
        return Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}