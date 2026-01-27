using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TestAdminProject.Domain.AggregatesModel.UserAggregate;
using TestAdminProject.Web.Application.Commands.Identity.Admin.UserCommands;
using TestAdminProject.Web.AppPermissions;
using TestAdminProject.Web.Utils;

namespace TestAdminProject.Web.Endpoints.Identity.Admin.UserEndpoints;

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
/// 密码重置
/// </summary>
/// <param name="mediator"></param>
public class PasswordResetEndpoint(IMediator mediator) : Endpoint<PasswordResetRequest, ResponseData<PasswordResetResponse>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users"));
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

