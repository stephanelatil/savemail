namespace Backend.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Backend.Models;
using Backend.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Backend.Services;

[Route("oauth/google")]
public class OAuthGoogleController : Controller
{
    private readonly IMailBoxService _mailboxService;
    private readonly IOAuthCredentialsService _oAuthCredentialsService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<OAuthGoogleController> _logger;

    public OAuthGoogleController(IMailBoxService mailboxService,
                                UserManager<AppUser> userManager,
                                ILogger<OAuthGoogleController> logger,
                                IOAuthCredentialsService oAuthCredentialsService)
    {
        this._mailboxService = mailboxService;
        this._oAuthCredentialsService = oAuthCredentialsService;
        this._userManager = userManager;
        this._logger = logger;
    }

    [Authorize] //only a logged in user can add an oauth token to a mailbox
    [HttpGet("login")]
    public async Task<IActionResult> LoginWithGoogle()
    {

        var user = await this._userManager.GetUserAsync(this.User);
        if (user?.Id is null)
            return this.Forbid();
        var mailbox = await this._mailboxService.CreateMailBoxAsync(
                            new UpdateMailBox(){Provider = ImapProvider.Google}, user);

        var properties = new AuthenticationProperties()
        {
            RedirectUri = this.Url.Action("GoogleCallback", mailbox.Id)
        };
        return this.Challenge(properties, "Google");
    }

    [Authorize] //only a logged in user can add an oauth token to a mailbox
    [HttpGet("relogin/{mailboxId}")]
    public async Task<IActionResult> ReLoginWithGoogle(int mailboxId)
    {
        var mailbox = await this._mailboxService.GetMailboxByIdAsync(mailboxId);
        if (mailbox is null)
            return this.NotFound();

        var user = await this._userManager.GetUserAsync(this.User);
        if (user?.Id is null || mailbox.OwnerId != user.Id)
            return this.Forbid();

        var properties = new AuthenticationProperties()
        {
            RedirectUri = this.Url.Action("GoogleCallback", mailboxId)
        };
        return this.Challenge(properties, "Google");
    }

    [Authorize]
    [HttpGet("callback/{mailboxId}")]
    public async Task<IActionResult> GoogleCallback(int mailboxId)
    {
        var authenticateResult = await this.HttpContext.AuthenticateAsync("Google");
        if (!authenticateResult.Succeeded)
            return this.BadRequest("Google Authentication failed");
        // Extract Google tokens and save them securely, associated with the current user
        var accessToken = authenticateResult.Properties.GetTokenValue("access_token");
        var refreshToken = authenticateResult.Properties.GetTokenValue("refresh_token");

        if (accessToken is null)
            return this.BadRequest("Google Access Token not provided");

        try{
            await this._oAuthCredentialsService.CreateNewCredentials(OAuthCredentials.OAuthProvider.GoogleOAuth,
                                                            accessToken,
                                                            refreshToken ?? string.Empty,
                                                            mailboxId);
        }
        catch (Exception e){
            return this.BadRequest(e.Message);
        }

        return this.CreatedAtAction("GetMailBox","MailBoxController", new { id = mailboxId }, null);
    }
}
