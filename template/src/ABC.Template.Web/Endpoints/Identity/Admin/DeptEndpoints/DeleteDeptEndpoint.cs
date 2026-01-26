using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ABC.Template.Domain.AggregatesModel.DeptAggregate;
using ABC.Template.Web.Application.Commands.Identity.Admin.DeptCommands;
using ABC.Template.Web.AppPermissions;

namespace ABC.Template.Web.Endpoints.Identity.Admin.DeptEndpoints;

/// <summary>
/// 删除部门的请求模型
/// </summary>
/// <param name="Id">部门ID</param>
public record DeleteDeptRequest(DeptId Id);

/// <summary>
/// 删除部门的API端点
/// 该端点用于删除指定的部门（软删除）
/// </summary>
[Tags("Depts")]
public class DeleteDeptEndpoint(IMediator mediator) : Endpoint<DeleteDeptRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Delete("/api/admin/dept/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DeptDelete);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 删除指定的部门
    /// </summary>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(DeleteDeptRequest request,CancellationToken ct)
    {

        var command = new DeleteDeptCommand(request.Id);
        await mediator.Send(command, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
