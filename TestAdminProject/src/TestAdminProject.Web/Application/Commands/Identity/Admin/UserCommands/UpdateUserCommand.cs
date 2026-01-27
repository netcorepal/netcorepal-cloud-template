using FluentValidation;
using TestAdminProject.Domain.AggregatesModel.UserAggregate;
using TestAdminProject.Domain.AggregatesModel.DeptAggregate;
using TestAdminProject.Infrastructure;
using TestAdminProject.Infrastructure.Repositories;
using TestAdminProject.Domain;

namespace TestAdminProject.Web.Application.Commands.Identity.Admin.UserCommands;

/// <summary>
/// 更新用户命令
/// </summary>
public record UpdateUserCommand(UserId UserId, string Name, string Email, string Phone, string RealName, int Status, string Gender, int Age, DateTimeOffset BirthDate, DeptId DeptId, string DeptName, string PasswordHash) : ICommand<UserId>;

/// <summary>
/// 更新用户命令验证器
/// </summary>
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("用户ID不能为空");
        RuleFor(x => x.Name).NotEmpty().WithMessage("用户名不能为空");
    }
}

/// <summary>
/// 更新用户命令处理器
/// </summary>
public class UpdateUserCommandHandler(IUserRepository userRepository) : ICommandHandler<UpdateUserCommand, UserId>
{
    public async Task<UserId> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(request.UserId, cancellationToken) ??
                   throw new KnownException($"未找到用户，UserId = {request.UserId}", ErrorCodes.UserNotFound);

        user.UpdateUserInfo(request.Name, request.Phone, request.RealName, request.Status, request.Email, request.Gender, request.BirthDate);

        if (!string.IsNullOrEmpty(request.PasswordHash))
        {
            user.UpdatePassword(request.PasswordHash);
        }

        if (request.DeptId != new DeptId(0) && !string.IsNullOrEmpty(request.DeptName))
        {
            var dept = new UserDept(user.Id, request.DeptId, request.DeptName);
            user.AssignDept(dept);
        }

        return user.Id;
    }
}

