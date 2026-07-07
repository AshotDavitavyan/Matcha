using Application.Interfaces;
using Domain.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public record AddPictureCommand(int UserId, Stream Stream, string Filename, string ContentType, int ByteLength) : IRequest<int>;

public class AddPictureCommandHandler(IPictureStorage storage, IUserPictureRepository userRepository) : IRequestHandler<AddPictureCommand, int>
{
	const int MaxPictureMegabytes = 5;
	const int MaxPictureBytes = MaxPictureMegabytes * 1024 * 1024;
	public async Task<int> Handle(AddPictureCommand request, CancellationToken cancellationToken)
	{
		(string ContentType, string Label)[] allowedContentTypes = [("image/jpeg", "JPEG"), ("image/png", "PNG")];
		string[] allowedExtensions = [".jpg", ".jpeg", ".png"];

		if (request.ByteLength <= 0)
		{
			throw new InvalidPictureUploadException("Picture upload cannot be empty.");
		}

		if (request.ByteLength > MaxPictureBytes)
		{
			throw new InvalidPictureUploadException("Picture upload cannot be longer than " + MaxPictureMegabytes + "MB.");
		}

		if (allowedContentTypes.All(type => type.ContentType != request.ContentType))
		{
			throw new InvalidPictureUploadException($"Picture upload must be one of these file types: {string.Join(", ", allowedContentTypes.Select(type => type.Label))}");
		}

		if (!allowedExtensions.Contains(Path.GetExtension(request.Filename).ToLowerInvariant()))
		{
			throw new InvalidPictureUploadException($"Picture extension must be one of: {string.Join(", ", allowedExtensions)} ");
		}
		string url = await storage.Save(request.Stream, request.Filename, request.ContentType, cancellationToken);
		try
		{
			int id = await userRepository.AddPicture(request.UserId, url);
			return id;
		}
		catch (Exception ex) when (!(ex is OperationCanceledException))
		{
			await storage.Delete(url, cancellationToken);
			throw;
		}
	}
}
