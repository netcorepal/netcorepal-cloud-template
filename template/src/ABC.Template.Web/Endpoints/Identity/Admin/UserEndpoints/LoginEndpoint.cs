using System.Security.Claims;
using System.Text.Json;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NetCorePal.Extensions.Dto;
using NetCorePal.Extensions.Jwt;
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Web.Application.Commands.Identity.Admin.UserCommands;
using ABC.Template.Web.Application.Queries;
using ABC.Template.Domain;
using ABC.Template.Web.Utils;
using Serilog;

namespace ABC.Template.Web.Endpoints.Identity.Admin.UserEndpoints;

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, string RefreshToken, UserId UserId, string Name, string Email, string Roles, IEnumerable<string> PermissionCodes, DateTimeOffset TokenExpiryTime);

[Tags("Users")]
[HttpPost("/api/admin/user/login")]
[AllowAnonymous]
public class LoginEndpoint(IMediator mediator, UserQuery userQuery, IJwtProvider jwtProvider, IOptions<AppConfiguration> appConfiguration, RoleQuery roleQuery) : Endpoint<LoginRequest, ResponseData<LoginResponse>>
{
    private const string PermissionClaimType = "permissions";

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        Log.Information("用户登录尝试: Username={Username}", req.Username);
        
        var loginInfo = await userQuery.GetUserInfoForLoginAsync(req.Username, ct);
        
        if (loginInfo == null || !PasswordHasher.VerifyHashedPassword(req.Password, loginInfo.PasswordHash))
        {
            throw new KnownException("用户名或密码错误", ErrorCodes.UserNameOrPasswordError);
        }

        var nowTime = DateTimeOffset.UtcNow;
        var tokenExpiryTime = nowTime.AddMinutes(appConfiguration.Value.TokenExpiryInMinutes);
        var refreshToken = TokenGenerator.GenerateRefreshToken();
        var roles = loginInfo.UserRoles.Select(r => r.RoleId).ToList();
        var assignedPermissionCodes = roles.Count > 0
            ? await roleQuery.GetAssignedPermissionCodesAsync(roles, ct)
            : Enumerable.Empty<string>();
        var claims = BuildClaims(loginInfo, assignedPermissionCodes);
        var config = appConfiguration.Value;
        var token = await jwtProvider.GenerateJwtToken(
            new JwtData(
                config.JwtIssuer,
                config.JwtAudience,
                claims,
                nowTime.UtcDateTime,
                tokenExpiryTime.UtcDateTime),
            ct);
        var response = new LoginResponse(
            token,
            refreshToken,
            loginInfo.UserId,
            loginInfo.Name,
            loginInfo.Email,
            JsonSerializer.Serialize(roles) ?? "[]",
            assignedPermissionCodes,
            tokenExpiryTime
        );

        // 更新用户登录时间和刷新令牌
        var updateCmd = new UpdateUserLoginTimeCommand(loginInfo.UserId, nowTime, refreshToken);
        await mediator.Send(updateCmd, ct);

        Log.Information("用户登录成功: UserId={UserId}, Username={Username}, Email={Email}, RoleCount={RoleCount}, PermissionCount={PermissionCount}", 
            loginInfo.UserId, loginInfo.Name, loginInfo.Email, roles.Count, assignedPermissionCodes.Count());

        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }

    /// <summary>
    /// 构建JWT Claims
    /// </summary>
    private static List<Claim> BuildClaims(UserLoginInfoQueryDto loginInfo, IEnumerable<string> permissionCodes)
    {
        var userIdString = loginInfo.UserId.ToString();
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, loginInfo.Name),
            new(ClaimTypes.Email, loginInfo.Email),
            new(ClaimTypes.NameIdentifier, userIdString)
        };
       
        claims.AddRange(permissionCodes.Select(code => new Claim(PermissionClaimType, code)));

        return claims;
    }
}