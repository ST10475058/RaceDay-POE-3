using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Web.Services;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ApiResponse
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}

public class RaceDayApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContext;
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public RaceDayApiClient(IHttpContextAccessor httpContext, IConfiguration config)
    {
        _httpContext = httpContext;
        _http = new HttpClient
        {
            BaseAddress = new Uri(config["ApiBaseUrl"] ?? "https://localhost:7283/api/")
        };
    }

    private void AttachToken()
    {
        var token = _httpContext.HttpContext?.Session.GetString("JwtToken");
        _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        AttachToken();
        var res = await _http.GetAsync(endpoint);
        if (!res.IsSuccessStatusCode)
            return new ApiResponse<T> { IsSuccess = false, ErrorMessage = await res.Content.ReadAsStringAsync() };

        var data = await res.Content.ReadFromJsonAsync<T>(_json);
        return new ApiResponse<T> { IsSuccess = true, Data = data };
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object body)
    {
        AttachToken();
        var res = await _http.PostAsJsonAsync(endpoint, body);
        if (!res.IsSuccessStatusCode)
            return new ApiResponse<T> { IsSuccess = false, ErrorMessage = await res.Content.ReadAsStringAsync() };

        var data = await res.Content.ReadFromJsonAsync<T>(_json);
        return new ApiResponse<T> { IsSuccess = true, Data = data };
    }

    public async Task<ApiResponse> PostAsync(string endpoint, object body)
    {
        AttachToken();
        var res = await _http.PostAsJsonAsync(endpoint, body);
        return new ApiResponse
        {
            IsSuccess = res.IsSuccessStatusCode,
            ErrorMessage = res.IsSuccessStatusCode ? null : await res.Content.ReadAsStringAsync()
        };
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, HttpContent content)
    {
        AttachToken();
        var res = await _http.PostAsync(endpoint, content);
        if (!res.IsSuccessStatusCode)
            return new ApiResponse<T> { IsSuccess = false, ErrorMessage = await res.Content.ReadAsStringAsync() };

        var data = await res.Content.ReadFromJsonAsync<T>(_json);
        return new ApiResponse<T> { IsSuccess = true, Data = data };
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object body)
    {
        AttachToken();
        var res = await _http.PutAsJsonAsync(endpoint, body);
        if (!res.IsSuccessStatusCode)
            return new ApiResponse<T> { IsSuccess = false, ErrorMessage = await res.Content.ReadAsStringAsync() };

        var data = await res.Content.ReadFromJsonAsync<T>(_json);
        return new ApiResponse<T> { IsSuccess = true, Data = data };
    }

    public async Task<ApiResponse> PutAsync(string endpoint, object body)
    {
        AttachToken();
        var res = await _http.PutAsJsonAsync(endpoint, body);
        return new ApiResponse
        {
            IsSuccess = res.IsSuccessStatusCode,
            ErrorMessage = res.IsSuccessStatusCode ? null : await res.Content.ReadAsStringAsync()
        };
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        AttachToken();
        var res = await _http.DeleteAsync(endpoint);
        return res.IsSuccessStatusCode;
    }
}