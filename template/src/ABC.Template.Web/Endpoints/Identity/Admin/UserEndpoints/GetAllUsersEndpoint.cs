using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ABC.Template.Web.Application.Queries;
using ABC.Template.Web.AppPermissions;

namespace ABC.Template.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 获取所有用户信息的API端点
/// 该端点用于查询系统中的所有用户信息，支持分页、筛选和搜索
/// </summary>
[Tags("Users")]
public class GetAllUsersEndpoint(UserQuery userQuery) : Endpoint<UserQueryInput, ResponseData<PagedData<UserInfoQueryDto>>>
{
  
    public override void Configure()
    {
        Get("/api/admin/users");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserView);
    }

   
    public override async Task HandleAsync(UserQueryInput req, CancellationToken ct)
    {
        var result = await userQuery.GetAllUsersAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
