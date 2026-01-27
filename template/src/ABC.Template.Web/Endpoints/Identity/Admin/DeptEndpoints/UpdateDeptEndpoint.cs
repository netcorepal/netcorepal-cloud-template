using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ABC.Template.Domain.AggregatesModel.DeptAggregate;
using ABC.Template.Web.Application.Commands.Identity.Admin.DeptCommands;
using ABC.Template.Web.AppPermissions;

namespace ABC.Template.Web.Endpoints.Identity.Admin.DeptEndpoints;

/// <summary>
/// 更新部门的请求模型
/// </summary>
/// <param name="Id">部门ID</param>
/// <param name="Name">部门名称</param>
/// <param name="Remark">备注</param>
/// <param name="ParentId">父级部门ID，可为空表示顶级部门</param>
/// <param name="Status">状态（0=禁用，1=启用）</param>
public record UpdateDeptRequest(DeptId Id, string Name, string Remark, DeptId? ParentId, int Status);

/// <summary>
/// 更新部门的API端点
/// 该端点用于更新现有部门的信息
/// </summary>
[Tags("Depts")]
public class UpdateDeptEndpoint(IMediator mediator) : Endpoint<UpdateDeptRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Put("/api/admin/dept");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DeptEdit);
    }

    public override async Task HandleAsync(UpdateDeptRequest req, CancellationToken ct)
    {
        // 如果父级ID为空，则设置为根部门（ID为0）
        var command = new UpdateDeptCommand(
            req.Id,
            req.Name,
            req.Remark,
            req.ParentId ?? new DeptId(0),
            req.Status
        );
        await mediator.Send(command, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
