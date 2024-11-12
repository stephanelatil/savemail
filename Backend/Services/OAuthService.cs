using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using Backend.Models;
using Newtonsoft.Json.Linq;
using NuGet.ProjectModel;

namespace Backend.Services;

public interface IOAuthService
{
    public Task<bool> RefreshToken(OAuthCredentials credentials, string ownerId);
    public Task<string> GetEmail(string decryptedAccessToken, string userInfoUrl);
    public Task<string> GetEmail(OAuthCredentials credentials, string ownerId); 
    public Task SetNeedReauth(OAuthCredentials credentials);
}

public class OAuthService : IOAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ApplicationDBContext _context;
    private readonly TokenEncryptionService _tokenEncryptionService;
    private readonly Dictionary<ImapProvider, string> _clientId;
    private readonly Dictionary<ImapProvider, string> _clientSecret;

    public OAuthService(HttpClient httpClient, IConfiguration configuration, TokenEncryptionService tokenEncryptionService, ApplicationDBContext context)
    {
        this._context = context;
        this._httpClient = httpClient;
        this._tokenEncryptionService = tokenEncryptionService;

        this._clientId = [];
        this._clientSecret = [];
        this._clientId[ImapProvider.Google] = configuration.GetValue<string>("OAuth2:GOOGLE_CLIENT_ID") ?? string.Empty;
        this._clientSecret[ImapProvider.Google] = configuration.GetValue<string>("OAuth2:GOOGLE_CLIENT_SECRET") ?? string.Empty;
        //add other client id/secrets here with other providers
    }

    public async Task SetNeedReauth(OAuthCredentials credentials)
    {
        var entry = this._context.OAuthCredentials.Update(credentials);
        entry.Entity.NeedReAuth = true;
        await this._context.SaveChangesAsync();
    }

    public async Task<string> GetEmail(OAuthCredentials credentials, string ownerId)
    {
        HttpRequestMessage request = new (HttpMethod.Get, OAuthCredentials.UserProfileUrl(credentials.Provider));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", 
                                            this._tokenEncryptionService.Decrypt(credentials.AccessToken, credentials.OwnerMailboxId, ownerId));

        using HttpResponseMessage response = await this._httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
            string? email = jsonResponse["email"]?.ToString();
            if (email is not null)
                return email.ToLowerInvariant();
            throw new InvalidDataException("Unable to fetch email");
        }
        throw new AuthenticationException("Unable to get user email");
    }

    public async Task<string> GetEmail(string decryptedAccessToken, string userInfoUrl)
    {
        HttpRequestMessage request = new (HttpMethod.Get, userInfoUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", decryptedAccessToken);

        using HttpResponseMessage response = await this._httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
            string? email = jsonResponse["email"]?.ToString();
            if (email is not null)
                return email.ToLowerInvariant();
        }
        throw new AuthenticationException("Unable to get user email");
    }

    public async Task<bool> RefreshToken(OAuthCredentials credentials, string ownerId)
    {
        this._context.OAuthCredentials.Update(credentials);
        string? clientId = this._clientSecret.GetValueOrDefault(credentials.Provider);
        string? clientSecret = this._clientSecret.GetValueOrDefault(credentials.Provider);
        if (clientId is null || clientSecret is null)
            return false;

        HttpRequestMessage request = new(HttpMethod.Post, OAuthCredentials.RefreshUrl(credentials.Provider))
        {
            Content = new StringContent(
                $"client_id={clientId}&client_secret={clientSecret}&refresh_token={credentials.RefreshToken}&grant_type=refresh_token",
                Encoding.UTF8,
                "application/x-www-form-urlencoded")
        };

        try
        {
            var response = await this._httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                credentials.NeedReAuth = true;
                await this._context.SaveChangesAsync();
                return false;
            }

            var tokenResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
            if (tokenResponse is null || !tokenResponse.ContainsKey("access_token"))
            {
                credentials.NeedReAuth = true;
                await this._context.SaveChangesAsync();
                return false;
            }

            credentials.AccessToken = this._tokenEncryptionService.Encrypt(tokenResponse.GetValue<string>("access_token"),
                                                                            credentials.OwnerMailboxId, ownerId);
            if (tokenResponse.ContainsKey("expires_in"))
                credentials.AccessTokenValidity = DateTime.UtcNow
                                                    .AddMinutes(-10)
                                                    .AddSeconds(
                                                        tokenResponse.GetValue<int>("expires_in"));
            else
                credentials.AccessTokenValidity = DateTime.UtcNow.AddMinutes(50);
            credentials.NeedReAuth = false;
        }
        catch {
            credentials.NeedReAuth = true;
            await this._context.SaveChangesAsync();
            return false;
        }

        this._context.OAuthCredentials.Update(credentials);
        await this._context.SaveChangesAsync();
        return true;
    }
}