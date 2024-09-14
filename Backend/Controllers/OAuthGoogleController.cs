namespace Backend.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;

[Route("oauth/google")]
public class OAuthGoogleController : Controller
{
    private readonly ITaskManager _taskManager;
    private readonly ApplicationDBContext _context;

    private readonly IOAuthService _oAuthService;
    private readonly IOAuthCredentialsService _oAuthCredentialsService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<OAuthGoogleController> _logger;

    public OAuthGoogleController(ITaskManager taskManager,
                                UserManager<AppUser> userManager,
                                ILogger<OAuthGoogleController> logger,
                                IOAuthCredentialsService oAuthCredentialsService,
                                ApplicationDBContext context,
                                IOAuthService oAuthService)
    {
        this._taskManager = taskManager;
        this._oAuthCredentialsService = oAuthCredentialsService;
        this._userManager = userManager;
        this._oAuthService = oAuthService;
        this._logger = logger;
        this._context = context;
    }

    [Authorize] //only a logged in user can add an oauth token to a mailbox
    [HttpGet("login/{mailboxId?}")]
    public async Task<IActionResult> LoginWithGoogle(int? mailboxId, [FromQuery]string? next)
    {
        var user = await this._userManager.GetUserAsync(this.User);
        if (user?.Id is null)
            return this.Forbid();

        var properties = new AuthenticationProperties()
        {
            RedirectUri = mailboxId is null ? 
                        this.Url.Action(nameof(GoogleCallback)) :
                        this.Url.Action(nameof(GoogleCallback), mailboxId)
        };

        if (mailboxId.HasValue){
            var mb = await this._context.MailBox.Where(mb => mb.Id == mailboxId.Value).SingleOrDefaultAsync();
            if (mb is null)
                return this.NotFound("This mailbox does not exist");
            if (mb.OwnerId != user.Id)
                return this.Forbid("This mailbox does not belong to you");
            properties.SetString("login_hint", mb.Username);
        }

        if(next is not null && properties.RedirectUri is not null)
            properties.RedirectUri = QueryHelpers.AddQueryString(properties.RedirectUri,
                                                                 "next", next);

        return this.Challenge(properties, "Google");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mailboxId">The mailbox ID for which to update the oauth tokens for</param>
    /// <param name="next">A URL parameter where the frontend should be redirected after a login.
    /// Note that the mailbox ID will be appended to this url</param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("callback/{mailboxId?}")]
    public async Task<IActionResult> GoogleCallback(int? mailboxId, [FromQuery]string? next)
    {
        AppUser? user = await this._userManager.GetUserAsync(this.User);
        if (user?.Id is null)
            return this.Forbid();
            
        var authenticateResult = await this.HttpContext.AuthenticateAsync("Google");

        if (!authenticateResult.Succeeded)
        {
            this._logger.LogDebug("Authentication failed google auth for user {}", user.Id);
            return this.BadRequest("Google Authentication failed");
        }else
            this._logger.LogDebug("Successful google auth for user {}", user.Id);
        var accessToken = authenticateResult.Properties.GetTokenValue("access_token");
        var refreshToken = authenticateResult.Properties.GetTokenValue("refresh_token") ?? string.Empty;
        var expiresIn = authenticateResult.Properties.GetTokenValue("expires_in");

        //Mark expired 10min after expiry date to discourage use of expired token when syncing with server
        var validUntil = int.TryParse(expiresIn, out int remainingSeconds) ? DateTime.Now.AddSeconds(remainingSeconds).AddMinutes(-10):
                                                                             DateTime.Now.AddMinutes(40);

        if (accessToken is null)
            return this.BadRequest("Access Token is null");

        string email = await this._oAuthService.GetEmail(accessToken, OAuthCredentials.UserProfileUrl(ImapProvider.Google));

        MailBox? mailbox = null;
        if (mailboxId.HasValue)
            // if re-auth with known mailboxID
            mailbox = await this._context.MailBox.Where(mb => mb.Id == mailboxId && mb.OwnerId == user.Id)
                                                 .FirstOrDefaultAsync();
        else
            // auth withoug givin mailbox id, ensure mailbox does not already exist with this email for this user
            mailbox = await this._context.MailBox.Where(mb => mb.OwnerId == user.Id && mb.Username == email)
                                                 .FirstOrDefaultAsync();
        if (mailbox is null)
            mailbox = await this._oAuthCredentialsService.CreateNewMailboxWithCredentials(
                            ImapProvider.Google,
                            accessToken,
                            validUntil,
                            refreshToken,
                            user);
        else
            await this._oAuthCredentialsService.CreateNewCredentials(
                            ImapProvider.Google,
                            accessToken,
                            validUntil,
                            refreshToken,
                            mailbox.Id);
        //Enqueues mailbox for sync
        this._taskManager.EnqueueTask(mailbox.Id);
        if (next is not null)
            return this.Redirect($"{next.TrimEnd('/')}/{mailbox.Id}");
        else
            return this.Ok("Authentication Success. Return to your page continue");
    }
}
