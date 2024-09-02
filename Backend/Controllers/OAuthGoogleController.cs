namespace Backend.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Backend.Models;
using Backend.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;

[Route("oauth/google")]
public class OAuthGoogleController : Controller
{
    private readonly IMailBoxService _mailboxService;
    private readonly ApplicationDBContext _context;

    private readonly IOAuthService _oAuthService;
    private readonly IOAuthCredentialsService _oAuthCredentialsService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<OAuthGoogleController> _logger;

    public OAuthGoogleController(IMailBoxService mailboxService,
                                UserManager<AppUser> userManager,
                                ILogger<OAuthGoogleController> logger,
                                IOAuthCredentialsService oAuthCredentialsService,
                                ApplicationDBContext context,
                                IOAuthService oAuthService)
    {
        this._mailboxService = mailboxService;
        this._oAuthCredentialsService = oAuthCredentialsService;
        this._userManager = userManager;
        this._oAuthService = oAuthService;
        this._logger = logger;
        this._context = context;
    }

    [Authorize] //only a logged in user can add an oauth token to a mailbox
    [HttpGet("login/{mailboxId?}")]
    public async Task<IActionResult> LoginWithGoogle(int? mailboxId, [FromQuery]string mailboxUrlRedirect)
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

        if(mailboxUrlRedirect is not null && properties.RedirectUri is not null)
            properties.RedirectUri = QueryHelpers.AddQueryString(properties.RedirectUri,
                                                                 "mailboxUrlRedirect",
                                                                 mailboxUrlRedirect);

        return this.Challenge(properties, "Google");
    }

    [Authorize]
    [HttpGet("callback/{mailboxId?}")]
    public async Task<IActionResult> GoogleCallback(int? mailboxId, [FromQuery]string mailboxUrlRedirect)
    {
        this._logger.LogInformation("In callback with mailbox id = {}", mailboxId ?? 0);

        AppUser? user = await this._userManager.GetUserAsync(this.User);
        if (user?.Id is null)
            return this.Forbid();
            
        var authenticateResult = await this.HttpContext.AuthenticateAsync("Google");

        this._logger.LogInformation("After Auth");

        if (!authenticateResult.Succeeded)
            return this.BadRequest("Google Authentication failed");
        var accessToken = authenticateResult.Properties.GetTokenValue("access_token");
        var refreshToken = authenticateResult.Properties.GetTokenValue("refresh_token") ?? string.Empty;
    
        this._logger.LogInformation("Got tokens");

        if (accessToken is null)
            return this.BadRequest("Access Token is null");

        string email = await this._oAuthService.GetEmail(accessToken, OAuthCredentials.UserProfileUrl(ImapProvider.Google));
        this._logger.LogInformation("Email gotten from access token {email}", email);

        MailBox? mailbox = mailboxId.HasValue ? await this._context.MailBox.Where(mb => mb.Id == mailboxId && mb.OwnerId == user.Id)
                                                      .FirstOrDefaultAsync() : null;
        if (mailbox is null)
            mailbox = await this._oAuthCredentialsService.CreateNewMailboxWithCredentials(
                            ImapProvider.Google,
                            accessToken,
                            refreshToken,
                            user);
        else
            await this._oAuthCredentialsService.CreateNewCredentials(
                            ImapProvider.Google,
                            accessToken,
                            refreshToken,
                            mailbox.Id);

        return this.Redirect($"{mailboxUrlRedirect.TrimEnd('/')}/{mailbox.Id}");
    }
}
