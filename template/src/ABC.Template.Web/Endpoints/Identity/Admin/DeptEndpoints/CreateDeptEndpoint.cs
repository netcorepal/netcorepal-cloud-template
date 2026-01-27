using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ABC.Template.Domain.AggregatesModel.DeptAggregate;
using ABC.Template.Web.Application.Commands.Identity.Admin.DeptCommands;
using ABC.Template.Web.AppPermissions;

namespace ABC.Template.Web.Endpoints.Identity.Admin.DeptEndpoints;

/// <summary>
/// 创建部门的请求模型
/// </summary>
/// <param name="Name">部门名称</param>
/// <param name="Remark">备注</param>
/// <param name="ParentId">父级部门ID，可为空表示顶级部门</param>
/// <param name="Status">状态（0=禁用，1=启用）</param>
public record CreateDeptRequest(string Name, string Remark, DeptId? ParentId, int Status);

/// <summary>
/// 创建部门的响应模型
/// </summary>
/// <param name="Id">新创建的部门ID</param>
/// <param name="Name">部门名称</param>
/// <param name="Remark">备注</param>
public record CreateDeptResponse(DeptId Id, string Name, string Remark);

/// <summary>
/// 创建部门
/// </summary>
/// <param name="mediator"></param>
public class CreateDeptEndpoint(IMediator mediator) : Endpoint<CreateDeptRequest, ResponseData<CreateDeptResponse>>
{
    public override void Configure()
    {
        Tags("Depts");
        Description(b => b.AutoTagOverride("Depts"));
        Post("/api/admin/dept");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DeptCreate);
    }

    public override async Task HandleAsync(CreateDeptRequest req, CancellationToken ct)
    {
        var cmd = new CreateDeptCommand(req.Name, req.Remark, req.ParentId, req.Status);
        var deptId = await mediator.Send(cmd, ct);
        var response = new CreateDeptResponse(deptId, req.Name, req.Remark);
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}
