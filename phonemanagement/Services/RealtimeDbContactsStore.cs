using System.Net.Http.Json;
using System.Text.Json;
using PhoneBookApp.Models;

namespace phonemanagement.Services;

public sealed class RealtimeDbContactsStore : IContactsStore
{
    private readonly HttpClient _httpClient;
    private readonly string _databaseUrl;
    private readonly string? _authToken;

    public RealtimeDbContactsStore(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _databaseUrl = (config["Firebase:DatabaseUrl"] ?? string.Empty).TrimEnd('/');
        _authToken = config["Firebase:AuthToken"];

        if (string.IsNullOrWhiteSpace(_databaseUrl))
            throw new InvalidOperationException("Firebase DatabaseUrl is not configured.");
    }

    public async Task<List<Contact>> GetAllAsync()
    {
        var map = await GetContactsMapAsync();
        return map.Values.OrderBy(c => c.Id).ToList();
    }

    public async Task<Contact?> GetByIdAsync(int id)
    {
        var res = await _httpClient.GetAsync(BuildUrl($"contacts/{id}.json"));
        if (!res.IsSuccessStatusCode) return null;
        var contact = await res.Content.ReadFromJsonAsync<Contact>();
        return contact;
    }

    public async Task<List<Contact>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return await GetAllAsync();

        var all = await GetAllAsync();
        return all.Where(c =>
                (!string.IsNullOrWhiteSpace(c.Name) && c.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(c.Phone) && c.Phone.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(c.Email) && c.Email.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public async Task<Contact> AddAsync(Contact contact)
    {
        var nextId = await NextIdAsync();
        contact.Id = nextId;

        var res = await _httpClient.PutAsJsonAsync(BuildUrl($"contacts/{nextId}.json"), contact);
        res.EnsureSuccessStatusCode();
        return contact;
    }

    public async Task<bool> UpdateAsync(Contact contact)
    {
        var existing = await GetByIdAsync(contact.Id);
        if (existing is null) return false;

        var res = await _httpClient.PutAsJsonAsync(BuildUrl($"contacts/{contact.Id}.json"), contact);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await GetByIdAsync(id);
        if (existing is null) return false;

        var res = await _httpClient.DeleteAsync(BuildUrl($"contacts/{id}.json"));
        return res.IsSuccessStatusCode;
    }

    private async Task<int> NextIdAsync()
    {
        var all = await GetAllAsync();
        return all.Count == 0 ? 1 : all.Max(c => c.Id) + 1;
    }

    private async Task<Dictionary<string, Contact>> GetContactsMapAsync()
    {
        var res = await _httpClient.GetAsync(BuildUrl("contacts.json"));
        if (!res.IsSuccessStatusCode) return new Dictionary<string, Contact>();

        var json = await res.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(json) || json == "null")
            return new Dictionary<string, Contact>();

        var map = JsonSerializer.Deserialize<Dictionary<string, Contact>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return map ?? new Dictionary<string, Contact>();
    }

    private string BuildUrl(string path)
    {
        var url = $"{_databaseUrl}/{path}";
        if (!string.IsNullOrWhiteSpace(_authToken))
            url += url.Contains('?') ? $"&auth={_authToken}" : $"?auth={_authToken}";
        return url;
    }
}

