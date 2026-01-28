using FluentValidation;
using ABC.Template.Domain.AggregatesModel.DeptAggregate;
using ABC.Template.Infrastructure.Repositories;
using ABC.Template.Domain;

namespace ABC.Template.Web.Application.Commands.Identity.Admin.DeptCommands;

/// <summary>
/// 更新部门命令
/// </summary>
public record UpdateDeptCommand(DeptId Id, string Name, string Remark, DeptId ParentId, int Status) : ICommand;

public class UpdateDeptCommandValidator : AbstractValidator<UpdateDeptCommand>
{
    public UpdateDeptCommandValidator()
    {
        RuleFor(d => d.Name).NotEmpty().WithMessage("部门名称不能为空");
        RuleFor(d => d.Status).InclusiveBetween(0, 1).WithMessage("状态值必须为0或1");
    }
}

/// <summary>
/// 更新部门命令处理器
/// </summary>
public class UpdateDeptCommandHandler(IDeptRepository deptRepository) : ICommandHandler<UpdateDeptCommand>
{
    public async Task Handle(UpdateDeptCommand request, CancellationToken cancellationToken)
    {
        var dept = await deptRepository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException($"未找到部门，Id = {request.Id}", ErrorCodes.DeptNotFound);

        dept.UpdateInfo(request.Name, request.Remark, request.ParentId, request.Status);
    }
}
