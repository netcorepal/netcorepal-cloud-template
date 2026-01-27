using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TestAdminProject.Domain.AggregatesModel.UserAggregate;
using TestAdminProject.Domain.AggregatesModel.DeptAggregate;
using TestAdminProject.Web.Application.Queries;
using TestAdminProject.Domain;
using TestAdminProject.Web.AppPermissions;

namespace TestAdminProject.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 获取用户资料的请求模型
/// </summary>
/// <param name="UserId">要查询的用户ID</param>
public record GetUserProfileRequest(UserId UserId);

/// <summary>
/// 用户资料的响应模型
/// </summary>
/// <param name="UserId">用户ID</param>
/// <param name="Name">用户名</param>
/// <param name="Phone">电话号码</param>
/// <param name="Roles">用户角色列表</param>
/// <param name="RealName">真实姓名</param>
/// <param name="Status">用户状态</param>
/// <param name="Email">邮箱地址</param>
/// <param name="CreatedAt">创建时间</param>
/// <param name="Gender">性别</param>
/// <param name="Age">年龄</param>
/// <param name="BirthDate">出生日期</param>
    /// <param name="DeptId">部门ID（可为空）</param>
    /// <param name="DeptName">部门名称</param>
public record UserProfileResponse(UserId UserId, string Name, string Phone, IEnumerable<string> Roles, string RealName, int Status, string Email, DateTimeOffset CreatedAt, string Gender, int Age, DateTimeOffset BirthDate, DeptId? DeptId, string DeptName);

/// <summary>
/// 获取用户资料
/// </summary>
/// <param name="userQuery"></param>
public class GetUserProfileEndpoint(UserQuery userQuery) : Endpoint<GetUserProfileRequest, ResponseData<UserProfileResponse?>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users"));
        Get("/api/admin/user/profile/{userId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserView);
    }

    public override async Task HandleAsync(GetUserProfileRequest req, CancellationToken ct)
    {
        var userInfo = await userQuery.GetUserByIdAsync(req.UserId, ct);
        if (userInfo == null)
        {
            throw new KnownException("无效的用户", ErrorCodes.InvalidUser);
        }
        var response = new UserProfileResponse(
            userInfo.UserId,
            userInfo.Name,
            userInfo.Phone,
            userInfo.Roles,
            userInfo.RealName,
            userInfo.Status,
            userInfo.Email,
            userInfo.CreatedAt,
            userInfo.Gender,
            userInfo.Age,
            userInfo.BirthDate,
            userInfo.DeptId,
            userInfo.DeptName
        );
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

