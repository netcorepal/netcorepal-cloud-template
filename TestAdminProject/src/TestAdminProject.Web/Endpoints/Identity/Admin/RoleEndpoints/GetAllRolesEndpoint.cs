using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TestAdminProject.Web.Application.Queries;
using TestAdminProject.Web.AppPermissions;

namespace TestAdminProject.Web.Endpoints.Identity.Admin.RoleEndpoints;

/// <summary>
/// 获取所有角色
/// </summary>
/// <param name="roleQuery"></param>
public class GetAllRolesEndpoint(RoleQuery roleQuery) : Endpoint<RoleQueryInput, ResponseData<PagedData<RoleQueryDto>>>
{
   
    public override void Configure()
    {
        Tags("Roles");
        Description(b => b.AutoTagOverride("Roles"));
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

