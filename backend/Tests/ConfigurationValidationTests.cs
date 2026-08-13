using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tests;

[Collection(ApplicationConfigurationCollection.Name)]
public class ConfigurationValidationTests
{
	[Fact]
	public void MissingConnectionString_PreventsStartup()
	{
		AssertMissingConfiguration(
			"ConnectionStrings__DefaultConnection",
			"Jwt__SecretKey",
			"test-only-jwt-signing-key-with-32-bytes",
			"connection string");
	}

	[Fact]
	public void MissingJwtSecret_PreventsStartup()
	{
		AssertMissingConfiguration(
			"Jwt__SecretKey",
			"ConnectionStrings__DefaultConnection",
			"Host=localhost;Port=5432;Database=matcha_test;Username=test;Password=test",
			"Jwt:SecretKey");
	}

	private static void AssertMissingConfiguration(
		string missingVariable,
		string configuredVariable,
		string configuredValue,
		string expectedMessage)
	{
		string? originalMissingValue = Environment.GetEnvironmentVariable(missingVariable);
		string? originalConfiguredValue = Environment.GetEnvironmentVariable(configuredVariable);
		string? originalAspNetCoreEnvironment = Environment.GetEnvironmentVariable(
			"ASPNETCORE_ENVIRONMENT");

		try
		{
			Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
			Environment.SetEnvironmentVariable(missingVariable, null);
			Environment.SetEnvironmentVariable(configuredVariable, configuredValue);

			using var factory = new WebApplicationFactory<Program>();
			InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
				() => factory.CreateClient());

			Assert.Contains(expectedMessage, exception.Message, StringComparison.OrdinalIgnoreCase);
		}
		finally
		{
			Environment.SetEnvironmentVariable(missingVariable, originalMissingValue);
			Environment.SetEnvironmentVariable(configuredVariable, originalConfiguredValue);
			Environment.SetEnvironmentVariable(
				"ASPNETCORE_ENVIRONMENT",
				originalAspNetCoreEnvironment);
		}
	}
}
