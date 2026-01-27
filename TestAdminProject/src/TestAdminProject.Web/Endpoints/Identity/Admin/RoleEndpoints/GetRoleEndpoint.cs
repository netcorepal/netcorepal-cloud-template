using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TestAdminProject.Domain.AggregatesModel.RoleAggregate;
using TestAdminProject.Web.Application.Queries;
using TestAdminProject.Web.AppPermissions;

namespace TestAdminProject.Web.Endpoints.Identity.Admin.RoleEndpoints;

/// <summary>
/// 获取角色信息的请求模型
/// </summary>
/// <param name="Id">要查询的角色ID</param>
public record GetRoleRequest(RoleId Id);

/// <summary>
/// 获取角色
/// </summary>
/// <param name="roleQuery"></param>
public class GetRoleEndpoint(RoleQuery roleQuery) : Endpoint<GetRoleRequest, ResponseData<RoleQueryDto>>
{

    public override void Configure()
    {
        Tags("Roles");
        Description(b => b.AutoTagOverride("Roles"));
        Get("/api/admin/roles/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleView);
    }


    public override async Task HandleAsync(GetRoleRequest req, CancellationToken ct)
    {
        // 通过查询服务获取角色详细信息
        var roleInfo = await roleQuery.GetRoleByIdAsync(req.Id, ct);

        // 验证角色是否存在
        if (roleInfo == null)
        {
            await Send.NotFoundAsync(ct);
        }
        else
        {
            await Send.OkAsync(roleInfo!.AsResponseData(), cancellation: ct);
        }
    }
}

