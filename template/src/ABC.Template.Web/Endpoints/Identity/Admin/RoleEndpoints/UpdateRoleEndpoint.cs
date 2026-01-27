using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ABC.Template.Domain.AggregatesModel.RoleAggregate;
using ABC.Template.Web.Application.Commands.Identity.Admin.RoleCommands;
using ABC.Template.Web.AppPermissions;

namespace ABC.Template.Web.Endpoints.Identity.Admin.RoleEndpoints;

/// <summary>
/// 更新角色信息的请求模型
/// </summary>
/// <param name="RoleId">要更新的角色ID</param>
/// <param name="Name">新的角色名称</param>
/// <param name="Description">新的角色描述</param>
/// <param name="PermissionCodes">新的权限代码列表</param>
public record UpdateRoleInfoRequest(RoleId RoleId, string Name, string Description, IEnumerable<string> PermissionCodes);

/// <summary>
/// 更新角色
/// </summary>
/// <param name="mediator"></param>
public class UpdateRoleEndpoint(IMediator mediator) : Endpoint<UpdateRoleInfoRequest, ResponseData<bool>>
{
 
    public override void Configure()
    {
        Tags("Roles");
        Description(b => b.AutoTagOverride("Roles"));
        Put("/api/admin/roles/update");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleEdit);
    }

   
    public override async Task HandleAsync(UpdateRoleInfoRequest request, CancellationToken ct)
    {
        var cmd = new UpdateRoleInfoCommand(
            request.RoleId,           
            request.Name,            
            request.Description,      
            request.PermissionCodes   
        );
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

