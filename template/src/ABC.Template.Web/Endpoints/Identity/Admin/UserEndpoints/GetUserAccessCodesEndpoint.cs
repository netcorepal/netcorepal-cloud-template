using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Web.Application.Queries;
using ABC.Template.Domain;

namespace ABC.Template.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 获取当前用户权限码的API端点
/// 该端点用于获取当前登录用户的所有权限代码
/// </summary>
[Tags("Users")]
public class GetUserAccessCodesEndpoint(RoleQuery roleQuery, UserQuery userQuery) : EndpointWithoutRequest<ResponseData<IEnumerable<string>>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        Get("/api/admin/auth/codes");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // 从JWT token中获取用户ID
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out var userIdValue))
        {
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);
        }

        var userId = new UserId(userIdValue);

        // 获取用户信息（包含角色）
        var userInfo = await userQuery.GetUserInfoForLoginByIdAsync(userId, ct);
        if (userInfo == null)
        {
            throw new KnownException("用户不存在", ErrorCodes.UserNotFound);
        }

        // 获取用户角色ID列表
        var roleIds = userInfo.UserRoles.Select(r => r.RoleId).ToList();

        // 查询权限代码（如果用户没有角色，则返回空列表）
        var permissionCodes = roleIds.Count > 0
            ? await roleQuery.GetAssignedPermissionCodesAsync(roleIds, ct)
            : Enumerable.Empty<string>();

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(permissionCodes.AsResponseData(), cancellation: ct);
    }
}
