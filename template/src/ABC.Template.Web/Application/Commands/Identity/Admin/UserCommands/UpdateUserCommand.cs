﻿using FluentValidation;
using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Domain.AggregatesModel.DeptAggregate;
using ABC.Template.Infrastructure;
using ABC.Template.Infrastructure.Repositories;
using ABC.Template.Domain;
//#if (UseMongoDB)
using Microsoft.EntityFrameworkCore;
//#endif

namespace ABC.Template.Web.Application.Commands.Identity.Admin.UserCommands;

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
//#if (UseMongoDB)
/// MongoDB 不支持通过导航属性保存跨集合实体，需要直接操作 UserDepts 集合
public class UpdateUserCommandHandler(IUserRepository userRepository, ApplicationDbContext dbContext) : ICommandHandler<UpdateUserCommand, UserId>
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
            var existingDept = await dbContext.UserDepts
                .FirstOrDefaultAsync(ud => ud.UserId == request.UserId, cancellationToken);

            if (existingDept != null)
            {
                existingDept.UpdateDept(request.DeptId, request.DeptName);
            }
            else
            {
                var newDept = new UserDept(user.Id, request.DeptId, request.DeptName);
                await dbContext.UserDepts.AddAsync(newDept, cancellationToken);
            }
        }

        return user.Id;
    }
}
//#else
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
//#endif
