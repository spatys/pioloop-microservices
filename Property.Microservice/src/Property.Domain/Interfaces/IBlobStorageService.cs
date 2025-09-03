namespace Property.Domain.Interfaces;

public interface IBlobStorageService
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string propertyId);
    Task DeleteImageAsync(string imageUrl);
}
