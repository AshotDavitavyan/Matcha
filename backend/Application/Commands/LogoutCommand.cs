using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record LogoutCommand(int UserId) : IRequest;

public class LogoutCommandHandler(IAuthRepository userRepository) : IRequestHandler<LogoutCommand>
{
	public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
	{
		await userRepository.ClearRefreshToken(request.UserId);
	}
}
