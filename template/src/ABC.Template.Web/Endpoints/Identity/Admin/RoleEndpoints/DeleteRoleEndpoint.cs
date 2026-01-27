using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ABC.Template.Domain.AggregatesModel.RoleAggregate;
using ABC.Template.Web.Application.Commands.Identity.Admin.RoleCommands;
using ABC.Template.Web.AppPermissions;

namespace ABC.Template.Web.Endpoints.Identity.Admin.RoleEndpoints;

/// <summary>
/// 删除角色的请求模型
/// </summary>
/// <param name="RoleId">要删除的角色ID</param>
public record DeleteRoleRequest(RoleId RoleId);

/// <summary>
/// 删除角色的API端点
/// 该端点用于从系统中删除指定的角色（软删除）
/// </summary>
[Tags("Roles")]
public class DeleteRoleEndpoint(IMediator mediator) : Endpoint<DeleteRoleRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Delete("/api/admin/roles/{roleId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleDelete);
    }

 
    public override async Task HandleAsync(DeleteRoleRequest request, CancellationToken ct)
    {
        var command = new DeleteRoleCommand(request.RoleId);
        await mediator.Send(command, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

