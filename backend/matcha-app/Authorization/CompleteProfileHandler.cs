using System.Security.Claims;
using Domain.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace matcha_app.Authorization;

public class CompleteProfileHandler(IUserProfileRepository userProfileRepository) : AuthorizationHandler<CompleteProfileRequirement>
{
	protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CompleteProfileRequirement requirement)
	{
		Claim? claim = context.User.FindFirst("sub") ?? context.User.FindFirst(ClaimTypes.NameIdentifier);
		if (claim == null || !int.TryParse(claim.Value, out int userId))
		{
			return;
		}
		bool isProfileComplete = await userProfileRepository.IsProfileComplete(userId);
		if (isProfileComplete)
		{
			context.Succeed(requirement);
		}
	}
}