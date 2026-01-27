using FluentValidation;
using TestAdminProject.Domain.AggregatesModel.RoleAggregate;
using TestAdminProject.Infrastructure.Repositories;
using TestAdminProject.Web.Application.Queries;

namespace TestAdminProject.Web.Application.Commands.Identity.Admin.RoleCommands;

public record CreateRoleCommand(string Name, string Description, IEnumerable<string> PermissionCodes) : ICommand<RoleId>;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator(RoleQuery roleQuery)
    {
        RuleFor(r => r.Name).NotEmpty().WithMessage("角色名称不能为空");
        RuleFor(r => r.Description).MaximumLength(200).WithMessage("角色描述长度不能超过200个字符");
        RuleFor(r => r.Name).MustAsync(async (n, ct) => !await roleQuery.DoesRoleExist(n, ct))
            .WithMessage(r => $"该角色已存在，Name={r.Name}");
    }
}

public class CreateRoleCommandHandler(IRoleRepository roleRepository) : ICommandHandler<CreateRoleCommand, RoleId>
{
    public async Task<RoleId> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var permissions = request.PermissionCodes.Select(perm => new RolePermission(perm));

        var role = new Role(request.Name, request.Description, permissions);

        await roleRepository.AddAsync(role, cancellationToken);

        return role.Id;
    }
}

