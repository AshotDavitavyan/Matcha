using System.Net;
using System.Net.Http.Json;
using Application.Dtos.UserDtos;
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

    [Fact]
    public async Task CreateUser_ReturnsCreated()
    {
        var dto = new CreateUserDto
        {
            Username = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/users", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var response = await _client.GetAsync("/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingUser_ReturnsOk()
    {
        var createDto = new CreateUserDto
        {
            Username = "getbyiduser",
            FirstName = "Get",
            LastName = "ById",
            Email = "getbyid@example.com",
            Password = "Password123!"
        };
        var createResponse = await _client.PostAsJsonAsync("/users", createDto);
        var id = await createResponse.Content.ReadFromJsonAsync<int>();

        var response = await _client.GetAsync($"/users/{id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.Equal("getbyiduser", user!.Username);
    }

    [Fact]
    public async Task GetById_NonExistingUser_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/users/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ReturnsOk()
    {
        var createDto = new CreateUserDto
        {
            Username = "updateuser",
            FirstName = "Update",
            LastName = "User",
            Email = "update@example.com",
            Password = "Password123!"
        };
        var createResponse = await _client.PostAsJsonAsync("/users", createDto);
        var id = await createResponse.Content.ReadFromJsonAsync<int>();

        var updateDto = new UpdateUserDto
        {
            Username = "updateduser",
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@example.com"
        };
        var response = await _client.PutAsJsonAsync($"/users/{id}", updateDto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePassword_ValidRequest_ReturnsNoContent()
    {
        var createDto = new CreateUserDto
        {
            Username = "passworduser",
            FirstName = "Password",
            LastName = "User",
            Email = "password@example.com",
            Password = "OldPassword123!"
        };
        var createResponse = await _client.PostAsJsonAsync("/users", createDto);
        var id = await createResponse.Content.ReadFromJsonAsync<int>();

        var updatePasswordDto = new UpdatePasswordDto
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        };
        var response = await _client.PutAsJsonAsync($"/users/{id}/password", updatePasswordDto);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePassword_WrongCurrentPassword_ReturnsBadRequest()
    {
        var createDto = new CreateUserDto
        {
            Username = "wrongpassuser",
            FirstName = "Wrong",
            LastName = "Pass",
            Email = "wrongpass@example.com",
            Password = "CorrectPassword123!"
        };
        var createResponse = await _client.PostAsJsonAsync("/users", createDto);
        var id = await createResponse.Content.ReadFromJsonAsync<int>();

        var updatePasswordDto = new UpdatePasswordDto
        {
            CurrentPassword = "WrongPassword123!",
            NewPassword = "NewPassword123!"
        };
        var response = await _client.PutAsJsonAsync($"/users/{id}/password", updatePasswordDto);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePassword_SamePassword_ReturnsBadRequest()
    {
        var createDto = new CreateUserDto
        {
            Username = "samepassuser",
            FirstName = "Same",
            LastName = "Pass",
            Email = "samepass@example.com",
            Password = "SamePassword123!"
        };
        var createResponse = await _client.PostAsJsonAsync("/users", createDto);
        var id = await createResponse.Content.ReadFromJsonAsync<int>();

        var updatePasswordDto = new UpdatePasswordDto
        {
            CurrentPassword = "SamePassword123!",
            NewPassword = "SamePassword123!"
        };
        var response = await _client.PutAsJsonAsync($"/users/{id}/password", updatePasswordDto);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
