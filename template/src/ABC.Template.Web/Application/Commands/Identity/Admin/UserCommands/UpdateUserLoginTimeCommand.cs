using ABC.Template.Domain.AggregatesModel.UserAggregate;
using ABC.Template.Infrastructure.Repositories;
using ABC.Template.Domain;

namespace ABC.Template.Web.Application.Commands.Identity.Admin.UserCommands;

public record UpdateUserLoginTimeCommand(UserId UserId, DateTimeOffset LoginTime, string RefreshToken) : ICommand;

public class UpdateUserLoginTimeCommandHandler(IUserRepository userRepository) : ICommandHandler<UpdateUserLoginTimeCommand>
{
    public async Task Handle(UpdateUserLoginTimeCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(request.UserId, cancellationToken)
                   ?? throw new KnownException($"未找到用户，UserId = {request.UserId}", ErrorCodes.UserNotFound);

        user.UpdateLastLoginTime(request.LoginTime);
        user.SetUserRefreshToken(request.RefreshToken);
    }
}

