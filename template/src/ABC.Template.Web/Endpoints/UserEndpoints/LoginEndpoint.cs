using System.Security.Claims;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using NetCorePal.Extensions.Dto;
using NetCorePal.Extensions.Jwt;

namespace ABC.Template.Web.Endpoints.UserEndpoints;

public record LoginRequest(string Username, string Password);

[Tags("Users")]
[HttpPost("/api/user/login")]
[AllowAnonymous]
public class LoginEndpoint(IJwtProvider jwtProvider) : Endpoint<LoginRequest, ResponseData<string>>
{
    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var jwt = await jwtProvider.GenerateJwtToken(
            new JwtData("netcorepal", "netcorepal",
                [new Claim("name", req.Username)],
                DateTime.Now, DateTime.Now.AddDays(1)));
        await Send.OkAsync(jwt.AsResponseData(), cancellation: ct);
    }
}