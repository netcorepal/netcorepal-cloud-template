using FluentValidation;
using ABC.Template.Domain.AggregatesModel.DeptAggregate;
using ABC.Template.Infrastructure.Repositories;
using ABC.Template.Web.Application.Queries;

namespace ABC.Template.Web.Application.Commands.Identity.Admin.DeptCommands;

/// <summary>
/// 创建部门命令
/// </summary>
public record CreateDeptCommand(string Name, string Remark, DeptId? ParentId, int Status) : ICommand<DeptId>;

public class CreateDeptCommandValidator : AbstractValidator<CreateDeptCommand>
{
    public CreateDeptCommandValidator(DeptQuery deptQuery)
    {
        RuleFor(d => d.Name).NotEmpty().WithMessage("部门名称不能为空");
        RuleFor(d => d.Name).MustAsync(async (n, ct) => !await deptQuery.DoesDeptExist(n, ct))
            .WithMessage(d => $"该部门已存在，Name={d.Name}");
        RuleFor(d => d.Status).InclusiveBetween(0, 1).WithMessage("状态值必须为0或1");
    }
}

/// <summary>
/// 创建部门命令处理器
/// </summary>
public class CreateDeptCommandHandler(IDeptRepository deptRepository) : ICommandHandler<CreateDeptCommand, DeptId>
{
    public async Task<DeptId> Handle(CreateDeptCommand request, CancellationToken cancellationToken)
    {
        var parentId = request.ParentId ?? new DeptId(0);
        var dept = new Dept(request.Name, request.Remark, parentId, request.Status);

        await deptRepository.AddAsync(dept, cancellationToken);

        return dept.Id;
    }
}
