using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Backend.Models;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Newtonsoft.Json.Linq;

namespace Backend.Services;

public interface IOAuthService
{
    public Task<bool> RefreshToken(OAuthCredentials credentials);
    public Task<string> GetEmail(OAuthCredentials credentials); 
}

public class OAuthService : IOAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ApplicationDBContext _context;
    private readonly string _clientId;
    private readonly string _clientSecret;

    public OAuthService(HttpClient httpClient, IConfiguration configuration, ApplicationDBContext context)
    {
        this._context = context;
        this._httpClient = httpClient;
        this._clientId = configuration.GetValue<string>("OAuth2__GOOGLE_CLIENT_ID") ?? string.Empty;
        this._clientSecret = configuration.GetValue<string>("OAuth2__GOOGLE_CLIENT_SECRET") ?? string.Empty;
    }

    public async Task<string> GetEmail(OAuthCredentials credentials)
    {
        HttpRequestMessage request = new (HttpMethod.Get, credentials.UserProfileUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credentials.AccessToken);

        using HttpResponseMessage response = await this._httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = JObject.Parse(await response.Content.ReadAsStringAsync());
            string? email = jsonResponse["email"]?.ToString();
            if (email is not null)
                return email;
        }
        throw new AuthenticationException("Unable to get user email");
    }

    public async Task<bool> RefreshToken(OAuthCredentials credentials)
    {
        this._context.OAuthCredentials.Update(credentials);
        if (credentials.RefreshTokenExpired)
        {
            credentials.NeedReAuth = true;
            await this._context.SaveChangesAsync();
            return false;
        }

        HttpRequestMessage request = new (HttpMethod.Post, credentials.RefreshUrl);

        var content = new StringContent($"client_id={this._clientId}&client_secret={this._clientSecret}&refresh_token={credentials.RefreshToken}&grant_type=refresh_token", Encoding.UTF8, "application/x-www-form-urlencoded");
        request.Content = content;

        var response = await this._httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            credentials.NeedReAuth = true;
            await this._context.SaveChangesAsync();
            return false;
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
        if (tokenResponse is null)
        {
            credentials.NeedReAuth = true;
            await this._context.SaveChangesAsync();
            return false;
        }

        credentials.AccessToken = tokenResponse.AccessToken;
        credentials.RefreshToken = tokenResponse.RefreshToken;
        credentials.NeedReAuth = false;

        this._context.OAuthCredentials.Update(credentials);
        await this._context.SaveChangesAsync();
        return true;
    }
}