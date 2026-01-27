using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TestAdminProject.Domain.AggregatesModel.RoleAggregate;
using TestAdminProject.Web.Application.Commands.Identity.Admin.RoleCommands;
using TestAdminProject.Web.AppPermissions;

namespace TestAdminProject.Web.Endpoints.Identity.Admin.RoleEndpoints;

/// <summary>
/// 激活角色的请求模型
/// </summary>
/// <param name="RoleId">要激活的角色ID</param>
public record ActivateRoleRequest(RoleId RoleId);

/// <summary>
/// 激活角色
/// </summary>
/// <param name="mediator"></param>
public class ActivateRoleEndpoint(IMediator mediator) : Endpoint<ActivateRoleRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Roles");
        Description(b => b.AutoTagOverride("Roles"));
        Put("/api/admin/roles/activate");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleEdit);
    }

    public override async Task HandleAsync(ActivateRoleRequest req, CancellationToken ct)
    {
        var cmd = new ActivateRoleCommand(req.RoleId);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
