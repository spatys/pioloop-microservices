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
        try
        {
            var folderPath = $"images/{propertyId}/{fileName}";
            
            // Utiliser l'API Vercel Blob v2 directement
            var url = "https://api.vercel.com/v2/blob";
            
            // Créer le contenu multipart
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(folderPath), "pathname");
            content.Add(new StringContent("public"), "access");
            
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            content.Add(streamContent, "file", fileName);
            
            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = content
            };
            
            // Headers requis pour Vercel Blob
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            
            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to upload image to Vercel Blob: {response.StatusCode} - {errorContent}");
            }
            
            var result = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(result);
            
            // Récupérer l'URL depuis la réponse de l'API
            return json.RootElement.GetProperty("url").GetString() 
                ?? throw new Exception("Failed to get image URL from response");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error uploading image: {ex.Message}", ex);
        }
    }

    public async Task DeleteImageAsync(string imageUrl)
    {
        try
        {
            var uri = new Uri(imageUrl);
            var pathname = uri.AbsolutePath.TrimStart('/'); // ex: images/123/photo.jpg
            var url = $"https://api.vercel.com/v2/blob/{pathname}";

            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            
            // Headers requis pour Vercel Blob
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to delete image: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting image: {ex.Message}", ex);
        }
    }
}
