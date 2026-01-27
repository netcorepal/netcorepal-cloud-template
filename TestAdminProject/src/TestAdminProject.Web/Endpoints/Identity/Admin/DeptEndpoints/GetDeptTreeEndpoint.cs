using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TestAdminProject.Web.Application.Queries;
using TestAdminProject.Web.AppPermissions;

namespace TestAdminProject.Web.Endpoints.Identity.Admin.DeptEndpoints;

/// <summary>
/// 获取部门树的请求模型
/// </summary>
/// <param name="IncludeInactive">是否包含非激活的部门</param>
public record GetDeptTreeRequest(bool IncludeInactive = false);

/// <summary>
/// 获取部门树
/// </summary>
/// <param name="deptQuery"></param>
public class GetDeptTreeEndpoint(DeptQuery deptQuery) : Endpoint<GetDeptTreeRequest, ResponseData<IEnumerable<DeptTreeDto>>>
{
    
    public override void Configure()
    {
        Tags("Depts");
        Description(b => b.AutoTagOverride("Depts"));
        Get("/api/admin/dept/tree");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DeptView);
    }

   
    public override async Task HandleAsync(GetDeptTreeRequest req, CancellationToken ct)
    {
        var tree = await deptQuery.GetDeptTreeAsync(req.IncludeInactive, ct);
        await Send.OkAsync(tree.AsResponseData(), cancellation: ct);
    }
}
