using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Tests;

public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
	private readonly HttpClient _client;

	public UsersControllerTests(WebApplicationFactory<Program> factory)
	{
		_client = factory.CreateClient();
	}

	[Theory]
	[InlineData("/users")]
	[InlineData("/users/1")]
	[InlineData("/users/1/profile")]
	public async Task ProtectedGetEndpoints_WithoutToken_ReturnUnauthorized(string url)
	{
		var response = await _client.GetAsync(url);

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async Task UpdateProfile_WithoutToken_ReturnsUnauthorized()
	{
		var response = await _client.PutAsJsonAsync("/users/1/profile", new
		{
			firstName = "Test",
			lastName = "User",
			email = "test@example.com",
			biography = "Biography",
			gender = 0,
			sexualPreference = 0,
			tags = Array.Empty<string>()
		});

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async Task AddPicture_WithoutToken_ReturnsUnauthorized()
	{
		using var content = new MultipartFormDataContent();
		content.Add(new ByteArrayContent([1, 2, 3]), "file", "profile.png");

		var response = await _client.PostAsync("/users/1/pictures", content);

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Theory]
	[InlineData("/users/1/pictures/10")]
	[InlineData("/users/1/pictures/10/profile")]
	public async Task PictureWriteEndpoints_WithoutToken_ReturnUnauthorized(string url)
	{
		HttpResponseMessage response = url.Contains("/profile")
			? await _client.PutAsync(url, null)
			: await _client.DeleteAsync(url);

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}
}
