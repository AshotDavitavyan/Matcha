using System.Security.Claims;
using Domain.Repositories;
using matcha_app.Authorization;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;
using Xunit;

namespace Tests;

public class CompleteProfileHandlerTests
{
	private static AuthorizationHandlerContext CreateContext(ClaimsPrincipal user, CompleteProfileRequirement requirement)
	{
		return new AuthorizationHandlerContext([requirement], user, resource: null);
	}

	private static ClaimsPrincipal UserWithClaim(string type, string value)
	{
		return new ClaimsPrincipal(new ClaimsIdentity([new Claim(type, value)], "TestAuth"));
	}

	[Fact]
	public async Task HandleAsync_MissingUserIdClaim_DoesNotSucceed()
	{
		var repository = Substitute.For<IUserRepository>();
		var requirement = new CompleteProfileRequirement();
		var context = CreateContext(new ClaimsPrincipal(new ClaimsIdentity("TestAuth")), requirement);
		var handler = new CompleteProfileHandler(repository);

		await handler.HandleAsync(context);

		Assert.False(context.HasSucceeded);
		await repository.DidNotReceiveWithAnyArgs().IsProfileComplete(default);
	}

	[Fact]
	public async Task HandleAsync_InvalidUserIdClaim_DoesNotSucceed()
	{
		var repository = Substitute.For<IUserRepository>();
		var requirement = new CompleteProfileRequirement();
		var context = CreateContext(UserWithClaim("sub", "not-an-int"), requirement);
		var handler = new CompleteProfileHandler(repository);

		await handler.HandleAsync(context);

		Assert.False(context.HasSucceeded);
		await repository.DidNotReceiveWithAnyArgs().IsProfileComplete(default);
	}

	[Fact]
	public async Task HandleAsync_IncompleteProfile_DoesNotSucceed()
	{
		var repository = Substitute.For<IUserRepository>();
		var requirement = new CompleteProfileRequirement();
		var context = CreateContext(UserWithClaim("sub", "7"), requirement);
		var handler = new CompleteProfileHandler(repository);

		repository.IsProfileComplete(7).Returns(Task.FromResult(false));

		await handler.HandleAsync(context);

		Assert.False(context.HasSucceeded);
		await repository.Received(1).IsProfileComplete(7);
	}

	[Fact]
	public async Task HandleAsync_CompleteProfile_Succeeds()
	{
		var repository = Substitute.For<IUserRepository>();
		var requirement = new CompleteProfileRequirement();
		var context = CreateContext(UserWithClaim("sub", "7"), requirement);
		var handler = new CompleteProfileHandler(repository);

		repository.IsProfileComplete(7).Returns(Task.FromResult(true));

		await handler.HandleAsync(context);

		Assert.True(context.HasSucceeded);
		await repository.Received(1).IsProfileComplete(7);
	}

	[Fact]
	public async Task HandleAsync_NameIdentifierClaim_UsesMappedUserId()
	{
		var repository = Substitute.For<IUserRepository>();
		var requirement = new CompleteProfileRequirement();
		var context = CreateContext(UserWithClaim(ClaimTypes.NameIdentifier, "9"), requirement);
		var handler = new CompleteProfileHandler(repository);

		repository.IsProfileComplete(9).Returns(Task.FromResult(true));

		await handler.HandleAsync(context);

		Assert.True(context.HasSucceeded);
		await repository.Received(1).IsProfileComplete(9);
	}
}
