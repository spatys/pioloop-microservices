using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Property.Domain.Interfaces;

namespace Property.Infrastructure.Services;

public interface IImageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName, string propertyId);
    Task<bool> DeleteImageAsync(string imageUrl);
    Task<bool> DeletePropertyImagesAsync(string propertyId);
}

public class ImageService : IImageService
{
    private readonly HttpClient _httpClient;
    private readonly string _blobApiUrl;
    private readonly string _blobToken;
    private readonly string _publicUrl;

    public ImageService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _blobApiUrl = "https://blob.vercel-storage.com";
        _blobToken = configuration["BLOB_READ_WRITE_TOKEN"] ?? throw new InvalidOperationException("BLOB_READ_WRITE_TOKEN not configured");
        _publicUrl = "https://9orgtoluacxop7hw.public.blob.vercel-storage.com";
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string propertyId)
    {
        try
        {
            // Créer le nom du fichier avec le dossier de la propriété
            var blobPath = $"{propertyId}/{Guid.NewGuid()}_{fileName}";
            
            // Préparer la requête multipart
            using var formData = new MultipartFormDataContent();
            using var streamContent = new StreamContent(imageStream);
            formData.Add(streamContent, "file", fileName);

            // Ajouter les métadonnées
            var metadata = new Dictionary<string, string>
            {
                { "propertyId", propertyId },
                { "uploadedAt", DateTime.UtcNow.ToString("O") }
            };
            
            var metadataJson = JsonSerializer.Serialize(metadata);
            formData.Add(new StringContent(metadataJson), "metadata");

            // Configurer la requête
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_blobApiUrl}/upload")
            {
                Content = formData
            };
            
            request.Headers.Add("Authorization", $"Bearer {_blobToken}");
            request.Headers.Add("x-pathname", blobPath);

            // Envoyer la requête
            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to upload image: {response.StatusCode} - {errorContent}");
            }

            // Retourner l'URL publique
            return $"{_publicUrl}/{blobPath}";
        }
        catch (Exception ex)
        {
            throw new Exception($"Error uploading image: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        try
        {
            // Extraire le chemin du blob depuis l'URL
            var blobPath = imageUrl.Replace(_publicUrl + "/", "");
            
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_blobApiUrl}/delete")
            {
                Content = new StringContent(JsonSerializer.Serialize(new { pathname = blobPath }))
            };
            
            request.Headers.Add("Authorization", $"Bearer {_blobToken}");
            request.Headers.Add("Content-Type", "application/json");

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            // Log l'erreur mais ne pas faire échouer l'opération
            Console.WriteLine($"Error deleting image {imageUrl}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeletePropertyImagesAsync(string propertyId)
    {
        try
        {
            // Lister tous les fichiers dans le dossier de la propriété
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_blobApiUrl}/list?prefix={propertyId}/");
            request.Headers.Add("Authorization", $"Bearer {_blobToken}");

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            var listResponse = JsonSerializer.Deserialize<BlobListResponse>(content);
            
            if (listResponse?.Blobs == null)
            {
                return true;
            }

            // Supprimer chaque image
            var deleteTasks = listResponse.Blobs.Select(blob => DeleteImageAsync($"{_publicUrl}/{blob.Pathname}"));
            await Task.WhenAll(deleteTasks);
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting property images for {propertyId}: {ex.Message}");
            return false;
        }
    }

    private class BlobListResponse
    {
        public List<BlobInfo> Blobs { get; set; } = new();
    }

    private class BlobInfo
    {
        public string Pathname { get; set; } = string.Empty;
    }
}
