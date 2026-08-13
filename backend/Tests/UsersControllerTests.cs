using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Tests;

[Collection(ApplicationConfigurationCollection.Name)]
public class UsersControllerTests : IClassFixture<MatchaWebApplicationFactory>
{
	private readonly HttpClient _client;

	public UsersControllerTests(MatchaWebApplicationFactory factory)
	{
		_client = factory.CreateClient();
	}

	[Theory]
	[InlineData("/users")]
	[InlineData("/users/1")]
	public async Task ProtectedGetEndpoints_WithoutToken_ReturnUnauthorized(string url)
	{
		var response = await _client.GetAsync(url);

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	[Fact]
	public async Task UpdateUser_WithoutToken_ReturnsUnauthorized()
	{
		var response = await _client.PutAsJsonAsync("/users/1", new
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
