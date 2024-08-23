using System.Text;
using System.Text.Json;
using Backend.Models;
using Microsoft.AspNetCore.Authentication.BearerToken;

namespace Backend.Services;

public interface IOAuthService
{
    public Task<AccessTokenResponse?> RefreshToken(OAuthCredentials credentials);
}

public class OAuthService : IOAuthService
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;

    public OAuthService(HttpClient httpClient, IConfiguration configuration)
    {
        this._httpClient = httpClient;
        this._clientId = configuration.GetValue<string>("OAuth2__GOOGLE_CLIENT_ID") ?? string.Empty;
        this._clientSecret = configuration.GetValue<string>("OAuth2__GOOGLE_CLIENT_SECRET") ?? string.Empty;
    }

    public async Task<AccessTokenResponse?> RefreshToken(OAuthCredentials credentials)
    {
        if (credentials.RefreshTokenTokenExpired)
            return null;

        HttpRequestMessage request = new (HttpMethod.Post, credentials.RefreshUrl);

        var content = new StringContent($"client_id={this._clientId}&client_secret={this._clientSecret}&refresh_token={credentials.RefreshToken}&grant_type=refresh_token", Encoding.UTF8, "application/x-www-form-urlencoded");
        request.Content = content;

        var response = await this._httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(responseContent);

        // Update the access token in your database
        // await UpdateAccessTokenForUserAsync(tokenResponse.AccessToken);

        return tokenResponse;
    }
}