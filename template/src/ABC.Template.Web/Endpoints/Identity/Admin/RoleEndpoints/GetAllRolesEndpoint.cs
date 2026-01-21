using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ABC.Template.Web.Application.Queries;
using ABC.Template.Web.AppPermissions;

namespace ABC.Template.Web.Endpoints.Identity.Admin.RoleEndpoints;

/// <summary>
/// 获取所有角色的API端点
/// 该端点用于查询系统中的所有角色信息，支持分页和筛选
/// </summary>
[Tags("Roles")]
public class GetAllRolesEndpoint(RoleQuery roleQuery) : Endpoint<RoleQueryInput, ResponseData<PagedData<RoleQueryDto>>>
{
   
    public override void Configure()
    {
        Get("/api/admin/roles");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleView);
    }

    public override async Task HandleAsync(RoleQueryInput req, CancellationToken ct)
    {
        var roleInfo = await roleQuery.GetAllRolesAsync(req, ct);
        await Send.OkAsync(roleInfo.AsResponseData(), cancellation: ct);
    }
}

