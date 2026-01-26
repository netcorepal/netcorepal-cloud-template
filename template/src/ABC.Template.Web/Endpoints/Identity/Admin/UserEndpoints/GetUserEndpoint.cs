using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Web.Application.Queries;
using ABC.Template.Web.AppPermissions;

namespace ABC.Template.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 获取用户信息的请求模型
/// </summary>
/// <param name="Id">要查询的用户ID</param>
public record GetUserRequest(UserId Id);

/// <summary>
/// 获取用户信息的API端点
/// 该端点用于根据用户ID查询用户的详细信息
/// </summary>
[Tags("Users")]
public class GetUserEndpoint(UserQuery userQuery) : Endpoint<GetUserRequest, ResponseData<UserInfoQueryDto>>
{
    public override void Configure()
    {
        Get("/api/admin/users/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserView);
    }

    public override async Task HandleAsync(GetUserRequest req, CancellationToken ct)
    {
        var userInfo = await userQuery.GetUserByIdAsync(req.Id, ct);
        if (userInfo == null)
        {
            await Send.NotFoundAsync(ct);
        }
        else
        {
            await Send.OkAsync(userInfo.AsResponseData(), cancellation: ct);
        }
    }
}
