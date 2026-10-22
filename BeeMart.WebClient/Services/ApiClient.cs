

using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using BeeMart.WebClient.Models;

namespace BeeMart.WebClient.Services;

public class ApiClient(HttpClient http, IHttpContextAccessor accessor)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpContextAccessor _accessor = accessor;
    private readonly HttpClient _http = http;

    public Task<ApiResponse<T>> GetAsync<T>(string url) =>
        SendAsync<T>(HttpMethod.Get, url, null);

    public Task<ApiResponse<T>> PostAsync<T>(string url, object body) =>
        SendAsync<T>(HttpMethod.Post, url, body);


    private async Task<ApiResponse<T>> SendAsync<T>(HttpMethod method, string url, object? body)
    {
        var session = _accessor.HttpContext?.Session;
        var token = session?.GetString("token");

        using var request = new HttpRequestMessage(method, url);
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
            request.Content = JsonContent.Create(body, options: JsonOptions);

        try
        {
            using var response = await _http.SendAsync(request);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions) ?? throw new JsonException();
            result.StatusCode = (int)response.StatusCode;

            if (response.StatusCode == HttpStatusCode.Unauthorized && !string.IsNullOrEmpty(token))
                session!.Clear();

            return result;
        }
        catch (HttpRequestException)
        {
            return Fail<T>("Tidak dapat terhubung ke server API. Pastikan API sedang berjalan.");
        }
        catch (JsonException)
        {
            return Fail<T>("Respons dari server API tidak dikenali.");
        }
    }

    private static ApiResponse<T> Fail<T>(string message) =>
        new() { Success = false, Message = message, StatusCode = 503 };
}