using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetCorePal.Extensions.Dto;
using NetCorePal.Extensions.Jwt;

namespace ABC.Template.Web.Controllers;

[Route("user")]
public class UserController
{
    [Route("login")]
    [HttpPost]
    public async Task<ResponseData<string>> Login(string username, string password,
        [FromServices] IJwtProvider jwtProvider)
    {
        var jwt = await jwtProvider.GenerateJwtToken(
            new JwtData("netcorepal", "netcorepal",
                [new Claim("name", username)],
                DateTime.Now, DateTime.Now.AddDays(1)));
        return jwt.AsResponseData();
    }

    [Route("auth")]
    [HttpGet]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public Task<ResponseData<bool>> Auth()
    {
        return Task.FromResult(true.AsResponseData());
    }
}