using FluentValidation;
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Domain.AggregatesModel.DeptAggregate;
using ABC.Template.Infrastructure.Repositories;
using ABC.Template.Web.Application.Queries;
using Serilog;

namespace ABC.Template.Web.Application.Commands.Identity.Admin.UserCommands;

/// <summary>
/// 创建用户命令
/// </summary>
public record CreateUserCommand(string Name, string Email, string Password, string Phone, string RealName, int Status, string Gender, DateTimeOffset BirthDate, DeptId? DeptId, string? DeptName, IEnumerable<AssignAdminUserRoleQueryDto> RolesToBeAssigned) : ICommand<UserId>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(UserQuery userQuery)
    {
        RuleFor(u => u.Name).NotEmpty().WithMessage("用户名不能为空");
        RuleFor(u => u.Password).NotEmpty().WithMessage("密码不能为空");
        RuleFor(u => u.Name).MustAsync(async (n, ct) => !await userQuery.DoesUserExist(n, ct))
            .WithMessage(u => $"该用户已存在，Name={u.Name}");
    }
}

public class CreateUserCommandHandler(IUserRepository userRepository) : ICommandHandler<CreateUserCommand, UserId>
{
    public async Task<UserId> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        Log.Information("开始创建用户: {UserName}, Email: {Email}", request.Name, request.Email);

        var passwordHash = Utils.PasswordHasher.HashPassword(request.Password);

        var roles = request.RolesToBeAssigned
            .Select(r => new UserRole(r.RoleId, r.RoleName))
            .ToList();

        var user = new User(request.Name, request.Phone, passwordHash, roles, request.RealName, request.Status, request.Email, request.Gender, request.BirthDate);
        if (request.DeptId != null && !string.IsNullOrEmpty(request.DeptName))
        {
            var dept = new UserDept(user.Id, request.DeptId, request.DeptName);
            user.AssignDept(dept);
            Log.Debug("为用户分配部门: UserId={UserId}, DeptId={DeptId}, DeptName={DeptName}", user.Id, request.DeptId, request.DeptName);
        }

        await userRepository.AddAsync(user, cancellationToken);

        Log.Information("用户创建成功: UserId={UserId}, UserName={UserName}, Email={Email}, RoleCount={RoleCount}", 
            user.Id, request.Name, request.Email, roles.Count);

        return user.Id;
    }
}
