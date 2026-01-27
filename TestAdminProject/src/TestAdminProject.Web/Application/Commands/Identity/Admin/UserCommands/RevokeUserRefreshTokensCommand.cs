using TestAdminProject.Domain.AggregatesModel.UserAggregate;
using TestAdminProject.Infrastructure.Repositories;
using TestAdminProject.Domain;

namespace TestAdminProject.Web.Application.Commands.Identity.Admin.UserCommands;

/// <summary>
/// 撤销用户所有刷新令牌的命令（用于退出登录）
/// </summary>
public record RevokeUserRefreshTokensCommand(UserId UserId) : ICommand;

public class RevokeUserRefreshTokensCommandHandler(IUserRepository userRepository) : ICommandHandler<RevokeUserRefreshTokensCommand>
{
    public async Task Handle(RevokeUserRefreshTokensCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(request.UserId, cancellationToken)
                   ?? throw new KnownException($"未找到用户，UserId = {request.UserId}", ErrorCodes.UserNotFound);

        user.RevokeAllRefreshTokens();
    }
}
