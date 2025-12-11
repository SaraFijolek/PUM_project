using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using App1.Models;
using System.Collections.Generic;
using Xamarin.Essentials;


public class ApiClient
{
    readonly HttpClient _http;

    const string TokenKey = "auth_token";

    public const string DefaultBaseUrl = " ";
    public string BaseUrl { get; set; } = DefaultBaseUrl;
    public string AuthToken { get; private set; }

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public void SetAuthToken(string token)
    {
        AuthToken = token;
        _http.DefaultRequestHeaders.Authorization =
            string.IsNullOrWhiteSpace(token) ? null : new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task RestoreTokenAsync()
    {
        var stored = await SecureStorage.GetAsync(TokenKey);
        if (!string.IsNullOrWhiteSpace(stored))
            SetAuthToken(stored);
    }

    async Task SaveTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return;
        SetAuthToken(token);
        await SecureStorage.SetAsync(TokenKey, token);
    }

    public async Task<HttpResponseMessage> PostAsync(string path, object payload, bool authRequired = false)
    {
        if (authRequired && string.IsNullOrWhiteSpace(AuthToken))
            return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized) { ReasonPhrase = "Missing token" };

        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _http.PostAsync($"{BaseUrl}{path}", content);
    }

    public async Task<(bool ok, string message)> LoginAsync(string email, string password)
    {
        var res = await PostAsync("/auth/login", new { email, password });
        var body = await res.Content.ReadAsStringAsync();

        if (res.IsSuccessStatusCode)
        {
            var token = ExtractToken(body);
            await SaveTokenAsync(token);
            return (true, body);
        }

        return (false, body);
    }

    public async Task<(bool ok, string message)> RegisterAsync(string name, string email, string password)
    {
        var res = await PostAsync("/auth/register", new { name, email, password });
        var body = await res.Content.ReadAsStringAsync();
        if (res.IsSuccessStatusCode)
            return (true, body);
        return (false, body);
    }

    public async Task<(bool ok, string message)> UpdateProfileAsync(
            string name,
            string surname,
            System.DateTime birthDate,
            string gender,
            double? height,
            double? weight,
            string avatarBase64)
    {
        var res = await PostAsync(
            "/user/profile",
            new
            {
                name,
                surname,
                birthDate,
                gender,
                height,
                weight,
                avatar = avatarBase64
            },
            authRequired: true);

        var body = await res.Content.ReadAsStringAsync();
        if (res.IsSuccessStatusCode)
            return (true, body);
        return (false, body);
    }

    public async Task<(bool ok, string message)> ResetPasswordAsync(string email)
    {
        var res = await PostAsync("/auth/reset-password", new { email });
        var body = await res.Content.ReadAsStringAsync();
        if (res.IsSuccessStatusCode)
            return (true, body);
        return (false, body);
    }

    public async Task<(bool ok, string message)> CreateActivityAsync(object payload)
    {
        var res = await PostAsync("/activities", payload, authRequired: true);
        var body = await res.Content.ReadAsStringAsync();
        if (res.IsSuccessStatusCode)
            return (true, body);
        return (false, body);
    }

    public async Task<List<Activity>> GetActivitiesAsync()
    {
        var res = await _http.GetAsync($"{BaseUrl}/activities");
        if (!res.IsSuccessStatusCode)
            return new List<Activity>();

        var json = await res.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<Activity>>(json);
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync()
    {
        var res = await _http.GetAsync($"{BaseUrl}/leaderboard/weekly");
        if (!res.IsSuccessStatusCode)
            return new List<LeaderboardEntry>();

        var json = await res.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<LeaderboardEntry>>(json);
    }

    string ExtractToken(string body)
    {
        try
        {
            var obj = JsonConvert.DeserializeObject<dynamic>(body);
            if (obj?.token != null)
                return (string)obj.token;
        }
        catch { }
        return body;
    }
}
