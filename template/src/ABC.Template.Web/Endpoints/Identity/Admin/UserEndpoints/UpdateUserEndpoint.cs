using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Domain.AggregatesModel.DeptAggregate;
using ABC.Template.Web.Application.Commands.Identity.Admin.UserCommands;
using ABC.Template.Web.AppPermissions;
using ABC.Template.Web.Utils;

namespace ABC.Template.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 更新用户信息的请求模型
/// </summary>
/// <param name="UserId">要更新的用户ID</param>
/// <param name="Name">用户名</param>
/// <param name="Email">邮箱地址</param>
/// <param name="Phone">电话号码</param>
/// <param name="RealName">真实姓名</param>
/// <param name="Status">用户状态</param>
/// <param name="Gender">性别</param>
/// <param name="Age">年龄</param>
/// <param name="BirthDate">出生日期</param>
    /// <param name="DeptId">部门ID</param>
    /// <param name="DeptName">部门名称</param>
    /// <param name="Password">密码（可选，为空则不更新）</param>
public record UpdateUserRequest(UserId UserId, string Name, string Email, string Phone, string RealName, int Status, string Gender, int Age, DateTimeOffset BirthDate, DeptId DeptId, string DeptName, string Password);

/// <summary>
/// 更新用户信息的响应模型
/// </summary>
/// <param name="UserId">已更新的用户ID</param>
/// <param name="Name">用户名</param>
/// <param name="Email">邮箱地址</param>
public record UpdateUserResponse(UserId UserId, string Name, string Email);

/// <summary>
/// 更新用户
/// </summary>
/// <param name="mediator"></param>
public class UpdateUserEndpoint(IMediator mediator) : Endpoint<UpdateUserRequest, ResponseData<UpdateUserResponse>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users"));
        Put("/api/admin/user/update");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserEdit);
    }

    public override async Task HandleAsync(UpdateUserRequest request, CancellationToken ct)
    {
        var passwordHash = string.Empty;
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            passwordHash = PasswordHasher.HashPassword(request.Password);
        }
        var cmd = new UpdateUserCommand(
            request.UserId,
            request.Name,
            request.Email,
            request.Phone,
            request.RealName,
            request.Status,
            request.Gender,
            request.Age,
            request.BirthDate,
            request.DeptId,
            request.DeptName,
            passwordHash
        );
        var userId = await mediator.Send(cmd, ct);
        var response = new UpdateUserResponse(userId, request.Name, request.Email);
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

