using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Web.Application.Commands.Identity.Admin.UserCommands;
using Serilog;

namespace ABC.Template.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 用户退出登录
/// </summary>
/// <param name="mediator"></param>
public class LogoutEndpoint(IMediator mediator) : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users"));
        Post("/api/admin/auth/logout");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        AllowAnonymous(); // 允许匿名访问，因为token可能已过期
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // 尝试从JWT token中获取用户ID（如果token有效）
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userIdString) && long.TryParse(userIdString, out var userIdValue))
        {
            var userId = new UserId(userIdValue);
            
            try
            {
                // 撤销用户的所有刷新令牌，防止token被继续使用
                var revokeCmd = new RevokeUserRefreshTokensCommand(userId);
                await mediator.Send(revokeCmd, ct);
                
                Log.Information("用户退出登录成功: UserId={UserId}", userId);
            }
            catch (Exception ex)
            {
                // 即使撤销token失败，也记录日志但不影响退出流程
                Log.Warning(ex, "用户退出登录时撤销刷新令牌失败: UserId={UserId}", userId);
            }
        }
        else
        {
            Log.Information("用户退出登录: Token已失效或未提供");
        }

        // 返回成功响应
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
