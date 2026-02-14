﻿﻿﻿using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using App1.Models;
using System.Collections.Generic;
using Xamarin.Essentials;
using System;


public class ApiClient
{
    readonly HttpClient _http;

    const string TokenKey = "auth_token";

    public const string DefaultBaseUrl = "http://api.mkproj.space/api";
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

    public async Task LogoutAsync()
    {
        SetAuthToken(null);
        try { SecureStorage.Remove(TokenKey); } catch { }
        await Task.CompletedTask;
    }

    public async Task<HttpResponseMessage> PostAsync(string path, object payload, bool authRequired = false)
    {
        if (authRequired && string.IsNullOrWhiteSpace(AuthToken))
            return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized) { ReasonPhrase = "Missing token" };

        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _http.PostAsync($"{BaseUrl}{path}", content);
    }

    public async Task<HttpResponseMessage> PutAsync(string path, object payload, bool authRequired = false)
    {
        if (authRequired && string.IsNullOrWhiteSpace(AuthToken))
            return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized) { ReasonPhrase = "Missing token" };

        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _http.PutAsync($"{BaseUrl}{path}", content);
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
        var res = await PostAsync("/register", new {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            FirstName = name,
            LastName = "",
            BirthDate = System.DateTime.Today.AddYears(-20),
            Gender = "unknown",
            HeightCm = 170,
            WeightKg = 70.0,
            AvatarUrl = "",
            PreferredLanguage = "pl"
        });
        var body = await res.Content.ReadAsStringAsync();
        if (res.IsSuccessStatusCode)
            return (true, body);
        return (false, body);
    }
    public async Task<Profile> GetProfileAsync()
           
    {
        var res =  await _http.GetAsync($"{BaseUrl}/me");

        var body = await res.Content.ReadAsStringAsync();

         return JsonConvert.DeserializeObject<Profile>(body);
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
        var res = await PutAsync(
            "/me",
            new
            {
                FirstName = name,
                LastName = surname,
                BirthDate = birthDate,
                Gender = gender,
                HeightCm = height.HasValue ? (int?)Convert.ToInt32(Math.Round(height.Value)) : null,
                WeightKg = weight.HasValue ? (decimal?)Convert.ToDecimal(weight.Value) : null,
                AvatarUrl = string.IsNullOrWhiteSpace(avatarBase64) ? null : avatarBase64
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
        var res = await _http.GetAsync($"{BaseUrl}/rankings/weekly");
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
