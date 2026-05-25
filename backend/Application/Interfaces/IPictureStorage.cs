namespace Application.Interfaces;

public interface IPictureStorage
{
	Task<string> Save(Stream stream, string filename, string contentType, CancellationToken token);
	Task Delete(string url, CancellationToken token);
}