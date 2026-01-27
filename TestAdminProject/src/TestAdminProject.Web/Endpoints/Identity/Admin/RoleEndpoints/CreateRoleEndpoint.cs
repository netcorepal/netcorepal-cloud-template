using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TestAdminProject.Domain.AggregatesModel.RoleAggregate;
using TestAdminProject.Web.Application.Commands.Identity.Admin.RoleCommands;
using TestAdminProject.Web.AppPermissions;

namespace TestAdminProject.Web.Endpoints.Identity.Admin.RoleEndpoints;

/// <summary>
/// 创建角色的请求模型
/// </summary>
/// <param name="Name">角色名称</param>
/// <param name="Description">角色描述</param>
/// <param name="PermissionCodes">权限代码列表</param>
public record CreateRoleRequest(string Name, string Description, IEnumerable<string> PermissionCodes);

/// <summary>
/// 创建角色的响应模型
/// </summary>
/// <param name="RoleId">新创建的角色ID</param>
/// <param name="Name">角色名称</param>
/// <param name="Description">角色描述</param>
public record CreateRoleResponse(RoleId RoleId, string Name, string Description);

/// <summary>
/// 创建角色
/// </summary>
/// <param name="mediator"></param>
public class CreateRoleEndpoint(IMediator mediator) : Endpoint<CreateRoleRequest, ResponseData<CreateRoleResponse>>
{
    public override void Configure()
    {
        Tags("Roles");
        Description(b => b.AutoTagOverride("Roles"));
        Post("/api/admin/roles");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleCreate);
    }

    public override async Task HandleAsync(CreateRoleRequest req, CancellationToken ct)
    {
        var cmd = new CreateRoleCommand(req.Name, req.Description, req.PermissionCodes);
        var result = await mediator.Send(cmd, ct);
        var response = new CreateRoleResponse(result, req.Name, req.Description);
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

