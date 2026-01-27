using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using NetCorePal.Extensions.Dto;

namespace TestAdminProject.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 用户认证
/// </summary>
public class AuthEndpoint : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users"));
        Get("/api/admin/user/auth");
        AuthSchemes("Bearer");
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        return Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}