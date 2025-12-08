using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class ApiClient
{
    readonly HttpClient _http;
    public string BaseUrl { get; set; } = " ";

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<(bool ok, string message)> LoginAsync(string email, string password)
    {
        var payload = new { email, password };
        var json = JsonConvert.SerializeObject(payload);
        var res = await _http.PostAsync(BaseUrl + "/auth/login", new StringContent(json, Encoding.UTF8, "application/json"));
        if (res.IsSuccessStatusCode)
            return (true, await res.Content.ReadAsStringAsync());
        var err = await res.Content.ReadAsStringAsync();
        return (false, err);
    }

    public async Task<(bool ok, string message)> RegisterAsync(string name, string email, string password)
    {
        var payload = new { name, email, password };
        var json = JsonConvert.SerializeObject(payload);
        var res = await _http.PostAsync(BaseUrl + "/auth/register", new StringContent(json, Encoding.UTF8, "application/json"));
        if (res.IsSuccessStatusCode)
            return (true, await res.Content.ReadAsStringAsync());
        var err = await res.Content.ReadAsStringAsync();
        return (false, err);
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
        var payload = new
        {
            name,
            surname,
            birthDate,
            gender,
            height,
            weight,
            avatar = avatarBase64
        };

        var json = JsonConvert.SerializeObject(payload);
        var res = await _http.PostAsync(BaseUrl + "/user/profile", new StringContent(json, Encoding.UTF8, "application/json"));

        if (res.IsSuccessStatusCode)
            return (true, await res.Content.ReadAsStringAsync());

        var err = await res.Content.ReadAsStringAsync();
        return (false, err);

    }
    public async Task<(bool ok, string message)> ResetPasswordAsync(string email)
    {
        var payload = new { email };
        var json = JsonConvert.SerializeObject(payload);
        var res = await _http.PostAsync(BaseUrl + "/auth/reset-password", new StringContent(json, Encoding.UTF8, "application/json"));
        if (res.IsSuccessStatusCode)
            return (true, await res.Content.ReadAsStringAsync());
        var err = await res.Content.ReadAsStringAsync();
        return (false, err);
    }
}


