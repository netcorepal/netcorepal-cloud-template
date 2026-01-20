//#if (UseAdmin)
using FastEndpoints;
using MediatR;
using ABC.Template.Domain.AggregatesModel.RoleAggregate;
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Domain.AggregatesModel.DeptAggregate;
using ABC.Template.Web.Application.Commands.Identity.Admin.UserCommands;
using ABC.Template.Web.Application.Queries;
using ABC.Template.Web.Utils;

namespace ABC.Template.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 用户注册的请求模型
/// </summary>
/// <param name="Name">用户名</param>
/// <param name="Email">邮箱地址</param>
/// <param name="Password">密码</param>
/// <param name="Phone">电话号码</param>
/// <param name="RealName">真实姓名</param>
/// <param name="Status">用户状态</param>
/// <param name="Gender">性别</param>
/// <param name="Age">年龄</param>
/// <param name="BirthDate">出生日期</param>
    /// <param name="DeptId">部门ID（可选）</param>
    /// <param name="DeptName">部门名称（可选）</param>
    /// <param name="RoleIds">要分配的角色ID列表</param>
public record RegisterRequest(string Name, string Email, string Password, string Phone, string RealName, int Status, string Gender, int Age, DateTimeOffset BirthDate, DeptId? DeptId, string? DeptName, IEnumerable<RoleId> RoleIds);

/// <summary>
/// 用户注册的响应模型
/// </summary>
/// <param name="UserId">新创建的用户ID</param>
/// <param name="Name">用户名</param>
/// <param name="Email">邮箱地址</param>
public record RegisterResponse(UserId UserId, string Name, string Email);

/// <summary>
/// 用户注册的API端点
/// 该端点用于在系统中创建新的用户账户，支持角色分配和组织单位设置
/// </summary>
[Tags("Users")]
public class RegisterEndpoint(IMediator mediator, RoleQuery roleQuery) : Endpoint<RegisterRequest, ResponseData<RegisterResponse>>
{
    public override void Configure()
    {
        Post("/api/admin/user/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterRequest request, CancellationToken ct)
    {
        var rolesToBeAssigned = await roleQuery.GetAdminRolesForAssignmentAsync(request.RoleIds, ct);
        var passwordHash = PasswordHasher.HashPassword(request.Password);
        var cmd = new CreateUserCommand(
            request.Name,
            request.Email,
            passwordHash,
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
        var response = new RegisterResponse(userId, request.Name, request.Email);
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}
//#endif