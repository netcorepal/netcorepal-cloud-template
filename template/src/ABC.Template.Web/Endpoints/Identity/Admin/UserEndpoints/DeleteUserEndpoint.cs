using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Web.Application.Commands.Identity.Admin.UserCommands;
using ABC.Template.Web.AppPermissions;

namespace ABC.Template.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 删除用户的请求模型
/// </summary>
/// <param name="UserId">要删除的用户ID</param>
public record DeleteUserRequest(UserId UserId);

/// <summary>
/// 删除用户
/// </summary>
/// <param name="mediator"></param>
public class DeleteUserEndpoint(IMediator mediator) : Endpoint<DeleteUserRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users"));
        Delete("/api/admin/users/{userId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserDelete);
    }

    public override async Task HandleAsync(DeleteUserRequest request, CancellationToken ct)
    {
        var command = new DeleteUserCommand(request.UserId);
        await mediator.Send(command, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

