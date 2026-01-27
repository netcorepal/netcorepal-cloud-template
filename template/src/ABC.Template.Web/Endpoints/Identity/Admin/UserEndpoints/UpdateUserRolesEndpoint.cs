using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Domain.AggregatesModel.RoleAggregate;
using ABC.Template.Web.Application.Commands.Identity.Admin.UserCommands;
using ABC.Template.Web.Application.Queries;
using ABC.Template.Web.AppPermissions;

namespace ABC.Template.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 更新用户角色的请求模型
/// </summary>
/// <param name="UserId">要更新角色的用户ID</param>
/// <param name="RoleIds">要分配给用户的角色ID列表</param>
public record UpdateUserRolesRequest(UserId UserId, IEnumerable<RoleId> RoleIds);

/// <summary>
/// 更新用户角色的响应模型
/// </summary>
/// <param name="UserId">已更新角色的用户ID</param>
public record UpdateUserRolesResponse(UserId UserId);

/// <summary>
/// 更新用户角色
/// </summary>
/// <param name="mediator"></param>
/// <param name="roleQuery"></param>
public class UpdateUserRolesEndpoint(IMediator mediator, RoleQuery roleQuery) : Endpoint<UpdateUserRolesRequest, ResponseData<UpdateUserRolesResponse>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users"));
        Put("/api/admin/users/update-roles");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserRoleAssign);
    }

    public override async Task HandleAsync(UpdateUserRolesRequest request, CancellationToken ct)
    {
        var rolesToBeAssigned = await roleQuery.GetAdminRolesForAssignmentAsync(request.RoleIds, ct);
        var cmd = new UpdateUserRolesCommand(request.UserId, rolesToBeAssigned);
        await mediator.Send(cmd, ct);
        var response = new UpdateUserRolesResponse(request.UserId);
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

