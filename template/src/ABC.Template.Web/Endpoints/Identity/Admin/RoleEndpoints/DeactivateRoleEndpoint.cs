using FastEndpoints;
using FastEndpoints.Swagger;
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
/// 停用角色
/// </summary>
/// <param name="mediator"></param>
public class DeactivateRoleEndpoint(IMediator mediator) : Endpoint<DeactivateRoleRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Roles");
        Description(b => b.AutoTagOverride("Roles"));
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
