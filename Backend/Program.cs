using System.Text;
using System.Text.Json.Serialization;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddAuthorization();
builder.Configuration.AddEnvironmentVariables("SAVEMAIL__");
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.Development.json", optional:false);
else
    builder.Configuration.AddJsonFile("appsettings.json", optional:true);

//Check mandatory vars:
foreach (var envVar in new []{"ConnectionStrings:Password"})
    if (string.IsNullOrWhiteSpace(builder.Configuration.GetValue<string>(envVar)))
    {
        throw new ArgumentException(
            $"SAVEMAIL__{envVar.Replace(":","__")} variable is not set! "
            +"Set the environment variable or set it in the appsettings.json file:"
            +string.Join("->", envVar.Split(":",StringSplitOptions.RemoveEmptyEntries)));
    }

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(opt => {
    opt.AllowInputFormatterExceptionMessages = true;
    opt.JsonSerializerOptions.AllowTrailingCommas = false;
    opt.JsonSerializerOptions.Encoder = null;
    opt.JsonSerializerOptions.IgnoreReadOnlyFields = false;
    opt.JsonSerializerOptions.IgnoreReadOnlyProperties = false;
    opt.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//insert DBContext
builder.Services.AddDbContext<ApplicationDBContext>(opt =>{
    var connectionString = new StringBuilder();
    if (builder.Environment.IsDevelopment()){
        opt.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
        opt.EnableSensitiveDataLogging();
        opt.EnableDetailedErrors();
    }
    
    connectionString.Append($"Host={builder.Configuration.GetConnectionString("Host")};");
    connectionString.Append($"Username={builder.Configuration.GetConnectionString("Username")};");
    connectionString.Append($"Password={builder.Configuration.GetConnectionString("Password")};");
    connectionString.Append($"Database={builder.Configuration.GetConnectionString("Database") ?? "savemaildb"};");

    opt.UseNpgsql(connectionString.ToString());
});


// Setup Email sending service and if we require email confirmation
bool _request_email_confimation = false;
{
    string require_email_conf = builder.Configuration.GetValue<string?>("RequireEmailConfirmation", "false").Trim();
    if (!string.IsNullOrWhiteSpace(require_email_conf)){
        if ("true".Contains(require_email_conf, StringComparison.InvariantCultureIgnoreCase) ||
            "yes".Contains(require_email_conf, StringComparison.InvariantCultureIgnoreCase)||
            require_email_conf == "1")
            _request_email_confimation = true;
    }
}

if (!string.IsNullOrWhiteSpace(builder.Configuration.GetValue<string?>("SendGrid:Key", null)) &&
    !string.IsNullOrWhiteSpace(builder.Configuration.GetValue<string?>("SendGrid:FromEmail", null)))
{
    builder.Services.AddTransient<IEmailSender, SendGridEmailSender>();
    _request_email_confimation &= true;
}
else if (!string.IsNullOrWhiteSpace(builder.Configuration.GetValue<string?>("Brevo:Key", null)) &&
    !string.IsNullOrWhiteSpace(builder.Configuration.GetValue<string?>("Brevo:SenderId", null)))
{
    builder.Services.AddTransient<IEmailSender, BrevoEmailSender>();
    _request_email_confimation &= true;
}
else
    _request_email_confimation = false;
builder.Services.AddSingleton<ITokenEncryptionService, TokenEncryptionService>();

builder.Services.AddProblemDetails();
// Add User auth
var auth = builder.Services.AddAuthentication("Identity").AddCookie("Identity");

builder.Services.AddIdentityApiEndpoints<AppUser>()
    .AddEntityFrameworkStores<ApplicationDBContext>().AddDefaultTokenProviders();

//Removes need for email confimation (no email server yet)
builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.RequireUniqueEmail = true;
    // basically disable lockout
    options.Lockout.AllowedForNewUsers=false;
    options.Lockout.MaxFailedAccessAttempts = 99999999;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMilliseconds(1);
    //only need email confirmation if email server service is present
    options.SignIn.RequireConfirmedEmail = _request_email_confimation;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    // Secure enabled if login done over https otherwise http is accepted
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    // Inaccessible by JS
    options.Cookie.HttpOnly = true;
    // 5 days
    options.ExpireTimeSpan = TimeSpan.FromDays(10);
    // Ensure Cookie is auto-renewed if any authenticated call is made 5-10 days after last refresh
    options.SlidingExpiration = true;
});

//Setup google oauth
string? clientId = builder.Configuration["OAuth2:GOOGLE_CLIENT_ID"];
string? clientSecret = builder.Configuration["OAuth2:GOOGLE_CLIENT_SECRET"];
if (clientId is not null && clientSecret is not null)
    auth.AddGoogle("Google", options =>
        {
            options.AccessType = "offline";
            options.ClientId = clientId;
            options.ClientSecret = clientSecret;

            options.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/auth";
            options.TokenEndpoint = "https://oauth2.googleapis.com/token";
            options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";

            options.Scope.Add("https://mail.google.com/");
            options.Scope.Add("https://www.googleapis.com/auth/userinfo.email");

            // Use PKCE
            options.UsePkce = true;
            options.SaveTokens = true;
            options.SignInScheme = IdentityConstants.ExternalScheme;
        });


builder.Services.AddControllers();


//Add services for background tasks
builder.Services.AddSingleton<ITaskManager, TaskManager>();
builder.Services.AddHostedService<DailyScheduleService>();
builder.Services.AddTransient<IImapFetchTaskService, ImapFetchTaskService>();
builder.Services.AddScoped<IImapMailFetchService, ImapMailFetchService>();
builder.Services.AddScoped<IImapFolderFetchService, ImapFolderFetchService>();
builder.Services.AddScoped<IMailBoxImapCheck, MailboxImapCheck>();
builder.Services.AddScoped<HttpClient,HttpClient>();
builder.Services.AddScoped<IOAuthService, OAuthService>();

//Add Services to edit elements in the database
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFolderService, FolderService>();
builder.Services.AddScoped<IMailBoxService, MailBoxService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();
builder.Services.AddScoped<IOAuthCredentialsService, OAuthCredentialsService>();

const string _corsPolicyName = "SAVEMAIL_CORS";
string defined_host = Environment.GetEnvironmentVariable("HOST") ?? "127.0.0.1";
builder.Services.AddCors(opt => 
    opt.AddPolicy(_corsPolicyName,b =>
        {
            b.WithOrigins("https://localhost:3000",
                          "https://localhost:5005",
                          "http://localhost:3000",
                          "http://localhost:5005",
                          $"https://{defined_host}:3000",
                          $"https://{defined_host}:5005",
                          $"http://{defined_host}:3000",
                          $"http://{defined_host}:5005",
                          "https://accounts.google.com")
            .AllowAnyHeader()
            .AllowCredentials()
            .AllowAnyMethod();
            //todo Add cors also for hostname & port of the frond/backend given in env vars
        })
);


var app = builder.Build();

// Add the user of authenticated users
app.UseAuthentication();
app.UseAuthorization();

// Map user model
app.MapGroup("/api/auth").MapIdentityApi<AppUser>();
app.MapPost("/api/auth/Logout", async (SignInManager<AppUser> _signInManager) => {
    await _signInManager.SignOutAsync();
    return Results.Ok();
}).RequireAuthorization();

//HealthCheck endpoint
app.MapGet("/healthz", [DisableCors]() => Results.Ok());

//In case of 4xx error so it doesn't leak info
app.UseStatusCodePages(async context =>
{
    context.HttpContext.Response.ContentType = "application/json";
    var result = $"{{\"message\":\"{context.HttpContext.Response.StatusCode}\"}}";
    await context.HttpContext.Response.WriteAsync(result);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors( //disable cors on development
        x=>x.AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(origin => true)
            .AllowCredentials()
    );
}
else
{
    app.UseCors(_corsPolicyName);
    app.UseHttpsRedirection();
    // in case of 5xx error to avoid leaking info
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            string result = "{'message':'500: Internal Server Error'}";
            await context.Response.WriteAsync(result);
        });
    });
}

app.MapControllers();
{
    var scope_provider = app.Services.GetRequiredService<IServiceScopeFactory>();
    using var scope = scope_provider.CreateScope();
    using var context = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
    context?.Database?.Migrate();
}
app.Run();
