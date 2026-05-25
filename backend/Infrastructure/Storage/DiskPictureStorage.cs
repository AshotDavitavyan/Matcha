using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
namespace Infrastructure.Storage;

public class DiskPictureStorage(IWebHostEnvironment env) : IPictureStorage
{
	public async Task<string> Save(Stream stream, string filename, string contentType, CancellationToken token)
	{
		string webRootPath = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
		string? extension =  Path.GetExtension(filename);
		string uploadsPath = Path.Combine(webRootPath, "uploads");
		Directory.CreateDirectory(uploadsPath);
		string storedFileName = Guid.NewGuid() + extension;
		string destinationPath = Path.Combine(uploadsPath, storedFileName);
		await using FileStream destination = File.Create(destinationPath);
		await stream.CopyToAsync(destination, token);
		return "/uploads/" + storedFileName;
	}

	public Task Delete(string url, CancellationToken token)
	{
		string webRootPath = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
		string relativePath = url.TrimStart('/');
		string physicalPath = Path.Combine(webRootPath, relativePath);
		if (File.Exists(physicalPath))
		{
			File.Delete(physicalPath);
		}

		return Task.CompletedTask;
	}
}
