using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ABC.Template.Domain.AggregatesModel.RoleAggregate;
using ABC.Template.Web.Application.Commands.Identity.Admin.RoleCommands;
using ABC.Template.Web.AppPermissions;

namespace ABC.Template.Web.Endpoints.Identity.Admin.RoleEndpoints;

/// <summary>
/// 停用角色的请求模型
/// </summary>
/// <param name="RoleId">要停用的角色ID</param>
public record DeactivateRoleRequest(RoleId RoleId);

/// <summary>
/// 停用角色的API端点
/// 该端点用于停用已激活的角色
/// </summary>
[Tags("Roles")]
public class DeactivateRoleEndpoint(IMediator mediator) : Endpoint<DeactivateRoleRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Put("/api/admin/roles/deactivate");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleEdit);
    }

    public override async Task HandleAsync(DeactivateRoleRequest req, CancellationToken ct)
    {
        var cmd = new DeactivateRoleCommand(req.RoleId);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
