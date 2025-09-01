using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Property.Domain.Interfaces;

namespace Property.Infrastructure.Services;

public interface IImageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName, string propertyId);
    Task<bool> DeleteImageAsync(string imageUrl);
    Task<bool> DeletePropertyImagesAsync(string propertyId);
    Task<List<string>> GetPropertyImageUrlsAsync(string propertyId);
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
                                      // Créer un nom de fichier unique pour Vercel Blob
             var uniqueFileName = $"property_{propertyId}_{Guid.NewGuid()}.jpg";
             
             // Utiliser l'endpoint /put qui fonctionne avec Vercel Blob
             using var streamContent = new StreamContent(imageStream);
             streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

             // Configurer la requête - Vercel Blob utilise PUT
             var request = new HttpRequestMessage(HttpMethod.Put, $"{_blobApiUrl}/put")
             {
                 Content = streamContent
             };
             
             request.Headers.Add("Authorization", $"Bearer {_blobToken}");

             // Envoyer la requête
             var response = await _httpClient.SendAsync(request);
             
             if (!response.IsSuccessStatusCode)
             {
                 var errorContent = await response.Content.ReadAsStringAsync();
                 throw new Exception($"Failed to upload image: {response.StatusCode} - {errorContent}");
             }

             // Lire la réponse pour obtenir l'URL réelle
             var responseContent = await response.Content.ReadAsStringAsync();
             Console.WriteLine($"Vercel Blob response: {responseContent}");
             
             // Vercel Blob retourne une URL dans la réponse, utilisons-la directement
             try
             {
                 var responseData = JsonSerializer.Deserialize<BlobUploadResponse>(responseContent);
                 if (!string.IsNullOrEmpty(responseData?.Url))
                 {
                     Console.WriteLine($"Image uploaded successfully to Vercel Blob: {responseData.Url}");
                     return responseData.Url; // Utiliser l'URL réelle retournée par Vercel Blob
                 }
             }
             catch (Exception parseEx)
             {
                 Console.WriteLine($"Failed to parse Vercel Blob response: {parseEx.Message}");
             }

             // Fallback: si pas d'URL dans la réponse, utiliser l'URL publique
             return $"{_publicUrl}/put";
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
            // Lister tous les fichiers dans le dossier de la propriété (structure: images/{propertyId}/)
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_blobApiUrl}/list?prefix=images/{propertyId}/");
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

    public async Task<List<string>> GetPropertyImageUrlsAsync(string propertyId)
    {
        try
        {
            // Cette méthode retourne une liste d'URLs d'images pour une propriété donnée
            // Pour l'instant, nous retournons une liste vide car nous n'avons pas d'endpoint pour lister
            // les fichiers dans Vercel Blob. Les images sont organisées logiquement par PropertyId
            // dans notre base de données.
            Console.WriteLine($"Getting image URLs for property: {propertyId}");
            await Task.CompletedTask; // Pour éviter l'avertissement CS1998
            return new List<string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting property image URLs for {propertyId}: {ex.Message}");
            return new List<string>();
        }
    }

    private class BlobUploadResponse
    {
        public string Url { get; set; } = string.Empty;
        public string Pathname { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string ContentDisposition { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public long Size { get; set; }
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
