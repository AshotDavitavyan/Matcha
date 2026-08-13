using Microsoft.AspNetCore.Mvc.Testing;

namespace Tests;

public class MatchaWebApplicationFactory : WebApplicationFactory<Program>
{
	private readonly string? _originalConnectionString;
	private readonly string? _originalJwtSecret;
	private readonly string? _originalAspNetCoreEnvironment;

	public MatchaWebApplicationFactory()
	{
		_originalConnectionString = Environment.GetEnvironmentVariable(
			"ConnectionStrings__DefaultConnection");
		_originalJwtSecret = Environment.GetEnvironmentVariable("Jwt__SecretKey");
		_originalAspNetCoreEnvironment = Environment.GetEnvironmentVariable(
			"ASPNETCORE_ENVIRONMENT");

		Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
		Environment.SetEnvironmentVariable(
			"ConnectionStrings__DefaultConnection",
			"Host=localhost;Port=5432;Database=matcha_test;Username=test;Password=test");
		Environment.SetEnvironmentVariable(
			"Jwt__SecretKey",
			"test-only-jwt-signing-key-with-32-bytes");
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		Environment.SetEnvironmentVariable(
			"ConnectionStrings__DefaultConnection",
			_originalConnectionString);
		Environment.SetEnvironmentVariable("Jwt__SecretKey", _originalJwtSecret);
		Environment.SetEnvironmentVariable(
			"ASPNETCORE_ENVIRONMENT",
			_originalAspNetCoreEnvironment);
	}
}
