using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ABC.Template.Web.Application.Queries;
using ABC.Template.Web.AppPermissions;

namespace ABC.Template.Web.Endpoints.Identity.Admin.DeptEndpoints;

/// <summary>
/// 获取部门树的请求模型
/// </summary>
/// <param name="IncludeInactive">是否包含非激活的部门</param>
public record GetDeptTreeRequest(bool IncludeInactive = false);

/// <summary>
/// 获取部门树的API端点
/// 该端点用于查询系统中的部门树形结构
/// </summary>
[Tags("Depts")]
public class GetDeptTreeEndpoint(DeptQuery deptQuery) : Endpoint<GetDeptTreeRequest, ResponseData<IEnumerable<DeptTreeDto>>>
{
    
    public override void Configure()
    {
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
