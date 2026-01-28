using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ABC.Template.Web.Application.Queries;
using ABC.Template.Web.AppPermissions;

namespace ABC.Template.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 获取所有用户
/// </summary>
/// <param name="userQuery"></param>
public class GetAllUsersEndpoint(UserQuery userQuery) : Endpoint<UserQueryInput, ResponseData<PagedData<UserInfoQueryDto>>>
{
  
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users"));
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

