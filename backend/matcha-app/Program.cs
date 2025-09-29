using Application.Commands;
using Domain.Repositories;
using Infrastructure.Database;
using Infrastructure.Repositories;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(CreateUserCommand).Assembly));
builder.Services.AddSingleton(new DbConnectionFactory(connectionString));
builder.Services.AddScoped<IUserRepository, UserRepository>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowSpecificOrigin");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapScalarApiReference(options =>
{
    options.Title = "My API Reference";
    options.Theme = ScalarTheme.Moon; // Options: Default, Mars, Moon, Solar
});

app.Run();
