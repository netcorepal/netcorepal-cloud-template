using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using NetCorePal.Extensions.Dto;

namespace ABC.Template.Web.Endpoints.Identity.Admin.UserEndpoints;

[Tags("Users")]
[Authorize(AuthenticationSchemes = "Bearer")]
[HttpGet("/api/admin/user/auth")]
public class AuthEndpoint : EndpointWithoutRequest<ResponseData<bool>>
{
    public override Task HandleAsync(CancellationToken ct)
    {
        return Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}