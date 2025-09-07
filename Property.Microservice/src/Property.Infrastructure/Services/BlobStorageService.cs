using System.Net.Http.Headers;
using System.Text.Json;
using Property.Domain.Interfaces;

namespace Property.Infrastructure.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly HttpClient _httpClient;
    private readonly string _token;

    public BlobStorageService(HttpClient httpClient, string token, string blobUrl)
    {
        _httpClient = httpClient;
        _token = token;
        // blobUrl n'est plus utilisé car on récupère l'URL depuis la réponse de l'API
    }

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string propertyId)
    {
        return await UploadFileAsync(fileStream, fileName, propertyId, "images");
    }

    public async Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string propertyId)
    {
        return await UploadFileAsync(fileStream, fileName, propertyId, "documents");
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string propertyId)
    {
        var fileType = GetFileTypeFromExtension(fileName);
        return await UploadFileAsync(fileStream, fileName, propertyId, fileType);
    }

    private async Task<string> UploadFileAsync(Stream fileStream, string fileName, string propertyId, string fileType)
    {
        try
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var sanitizedFileName = SanitizeFileName(fileName);

            // 👇 folder style path
            var organizedFileName = $"{fileType}/{propertyId}/{timestamp}_{sanitizedFileName}";

            // Request upload URL from Vercel Blob
            var url = "https://api.vercel.com/v2/blob";

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(organizedFileName), "pathname");
            content.Add(new StringContent("public"), "access");

            // Read the stream to bytes for better content type handling
            var imageBytes = new byte[fileStream.Length];
            fileStream.Read(imageBytes, 0, imageBytes.Length);
            
            var contentType = GetContentType(fileName);
            var byteContent = new ByteArrayContent(imageBytes);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(byteContent, "file", organizedFileName);

            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to upload file to Vercel Blob: {response.StatusCode} - {errorContent}");
            }

            var result = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(result);

            return json.RootElement.GetProperty("url").GetString()
                ?? throw new Exception("Failed to get file URL from response");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error uploading file: {ex.Message}", ex);
        }
    }

    public async Task DeleteImageAsync(string imageUrl)
    {
        await DeleteFileAsync(imageUrl);
    }

    public async Task DeleteDocumentAsync(string documentUrl)
    {
        await DeleteFileAsync(documentUrl);
    }

    private async Task DeleteFileAsync(string fileUrl)
    {
        try
        {
            var uri = new Uri(fileUrl);
            var pathname = uri.AbsolutePath.TrimStart('/'); // ex: ob-xxxxx (Vercel Blob format)
            var url = $"https://api.vercel.com/v2/blob/{pathname}";

            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            
            // Headers requis pour Vercel Blob
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to delete file: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting file: {ex.Message}", ex);
        }
    }


    private string SanitizeFileName(string fileName)
    {
        // Remove or replace invalid characters for file names
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = fileName;
        
        foreach (var invalidChar in invalidChars)
        {
            sanitized = sanitized.Replace(invalidChar, '_');
        }
        
        // Remove any remaining spaces and replace with underscores
        sanitized = sanitized.Replace(" ", "_");
        
        // Ensure the filename is not too long
        if (sanitized.Length > 100)
        {
            var extension = Path.GetExtension(sanitized);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(sanitized);
            var maxNameLength = 100 - extension.Length;
            sanitized = nameWithoutExtension.Substring(0, Math.Min(nameWithoutExtension.Length, maxNameLength)) + extension;
        }
        
        return sanitized;
    }

    private string GetFileTypeFromExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" or ".svg" or ".tiff" or ".ico" => "images",
            ".pdf" or ".doc" or ".docx" or ".xls" or ".xlsx" or ".ppt" or ".pptx" or ".txt" or ".rtf" or ".zip" or ".rar" or ".7z" => "documents",
            _ => "documents" // Default to documents folder for unknown file types
        };
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".tiff" => "image/tiff",
            ".ico" => "image/x-icon",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".txt" => "text/plain",
            ".rtf" => "application/rtf",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            ".7z" => "application/x-7z-compressed",
            _ => "application/octet-stream"
        };
    }
}
