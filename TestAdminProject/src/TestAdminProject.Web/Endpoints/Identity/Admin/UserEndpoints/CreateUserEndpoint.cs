using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TestAdminProject.Domain.AggregatesModel.RoleAggregate;
using TestAdminProject.Domain.AggregatesModel.UserAggregate;
using TestAdminProject.Domain.AggregatesModel.DeptAggregate;
using TestAdminProject.Web.Application.Commands.Identity.Admin.UserCommands;
using TestAdminProject.Web.Application.Queries;
using TestAdminProject.Web.AppPermissions;
using TestAdminProject.Web.Utils;

namespace TestAdminProject.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 创建用户的请求模型
/// </summary>
/// <param name="Name">用户名</param>
/// <param name="Email">邮箱地址</param>
/// <param name="Password">密码</param>
/// <param name="Phone">电话号码</param>
/// <param name="RealName">真实姓名</param>
/// <param name="Status">用户状态（0=禁用，1=启用）</param>
/// <param name="Gender">性别</param>
/// <param name="BirthDate">出生日期</param>
/// <param name="DeptId">部门ID（可选）</param>
/// <param name="DeptName">部门名称（可选）</param>
/// <param name="RoleIds">要分配的角色ID列表</param>
public record CreateUserRequest(string Name, string Email, string Password, string Phone, string RealName, int Status, string Gender, DateTimeOffset BirthDate, DeptId? DeptId, string? DeptName, IEnumerable<RoleId> RoleIds);

/// <summary>
/// 创建用户的响应模型
/// </summary>
/// <param name="UserId">新创建的用户ID</param>
/// <param name="Name">用户名</param>
/// <param name="Email">邮箱地址</param>
public record CreateUserResponse(UserId UserId, string Name, string Email);

/// <summary>
/// 创建用户
/// </summary>
/// <param name="mediator"></param>
/// <param name="roleQuery"></param>
public class CreateUserEndpoint(IMediator mediator, RoleQuery roleQuery) : Endpoint<CreateUserRequest, ResponseData<CreateUserResponse>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users"));
        Post("/api/admin/users");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserCreate);
    }

    public override async Task HandleAsync(CreateUserRequest request, CancellationToken ct)
    {
        var rolesToBeAssigned = await roleQuery.GetAdminRolesForAssignmentAsync(request.RoleIds, ct);
        var cmd = new CreateUserCommand(
            request.Name,
            request.Email,
            request.Password,
            request.Phone,
            request.RealName,
            request.Status,
            request.Gender,
            request.BirthDate,
            request.DeptId,
            request.DeptName,
            rolesToBeAssigned
        );
        var userId = await mediator.Send(cmd, ct);
        var response = new CreateUserResponse(userId, request.Name, request.Email);
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}
