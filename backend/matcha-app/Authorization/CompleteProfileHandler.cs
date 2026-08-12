using System.Security.Claims;
using Domain.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace matcha_app.Authorization;

public class CompleteProfileHandler(IUserRepository userRepository) : AuthorizationHandler<CompleteProfileRequirement>
{
	protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CompleteProfileRequirement requirement)
	{
		Claim? claim = context.User.FindFirst("sub") ?? context.User.FindFirst(ClaimTypes.NameIdentifier);
		if (claim == null || !int.TryParse(claim.Value, out int userId))
		{
			return;
		}
		var token = context.Resource is HttpContext httpContext ? httpContext.RequestAborted : CancellationToken.None;
		bool isProfileComplete = await userRepository.IsProfileComplete(userId, token);
		if (isProfileComplete)
		{
			context.Succeed(requirement);
		}
	}
}