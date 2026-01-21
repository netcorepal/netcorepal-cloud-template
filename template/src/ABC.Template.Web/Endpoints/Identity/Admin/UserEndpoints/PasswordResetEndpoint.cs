using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Web.Application.Commands.Identity.Admin.UserCommands;
using ABC.Template.Web.AppPermissions;
using ABC.Template.Web.Utils;

namespace ABC.Template.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 密码重置的请求模型
/// </summary>
/// <param name="UserId">需要重置密码的用户ID</param>
public record PasswordResetRequest(UserId UserId);

/// <summary>
/// 密码重置的响应模型
/// </summary>
/// <param name="UserId">已重置密码的用户ID</param>
public record PasswordResetResponse(UserId UserId);

/// <summary>
/// 密码重置的API端点
/// 该端点用于重置指定用户的密码为默认密码（123456）
/// </summary>
[Tags("Users")]
public class PasswordResetEndpoint(IMediator mediator) : Endpoint<PasswordResetRequest, ResponseData<PasswordResetResponse>>
{
    public override void Configure()
    {
        Put("/api/admin/user/password-reset");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserEdit);
    }

    public override async Task HandleAsync(PasswordResetRequest request, CancellationToken ct)
    {
        var passwordHash = PasswordHasher.HashPassword("123456");
        var cmd = new PasswordResetCommand(request.UserId, passwordHash);
        var userId = await mediator.Send(cmd, ct);
        var response = new PasswordResetResponse(userId);
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

