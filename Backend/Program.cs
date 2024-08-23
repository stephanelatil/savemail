using System.Text;
using System.Text.Json.Serialization;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddAuthorization();
builder.Configuration.AddEnvironmentVariables("SAVEMAIL_");

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
    
    connectionString.Append($"Host={builder.Configuration.GetConnectionString("Host")};");
    connectionString.Append($"Username={builder.Configuration.GetConnectionString("Username")};");
    connectionString.Append($"Password={builder.Configuration.GetConnectionString("Password")};");
    connectionString.Append($"Database={builder.Configuration.GetConnectionString("Database") ?? "savemaildb"};");

    opt.UseNpgsql(connectionString.ToString());
    opt.EnableSensitiveDataLogging(false);
});


//Setup SendGrid
//string a = builder.Configuration.GetRequiredSection("SendGrid")['SendGridKey'];

builder.Services.AddProblemDetails();
// Add User auth
builder.Services.AddAuthentication(opt => 
                    opt.DefaultAuthenticateScheme=CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

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
    options.SignIn.RequireConfirmedEmail = false;
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
string? clientId = builder.Configuration.GetValue<string>("OAuth2__GOOGLE_CLIENT_ID");
string? clientSecret = builder.Configuration.GetValue<string>("OAuth2__GOOGLE_CLIENT_SECRET");
if (clientId is not null && clientSecret is not null)
    builder.Services.AddAuthentication()
        .AddOAuth("Google", options =>
        {
            options.ClientId = clientId;
            options.ClientSecret = clientSecret;
            options.CallbackPath = new PathString("/signin-google");

            options.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/auth";
            options.TokenEndpoint = "https://oauth2.googleapis.com/token";
            options.UserInformationEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";

            options.Scope.Add("https://www.googleapis.com/auth/gmail.readonly");
            options.Scope.Add("https://www.googleapis.com/auth/gmail.metadata");

            // Use PKCE
            options.UsePkce = true;
            options.SaveTokens = true;

            options.Events = new OAuthEvents()
            {
                OnCreatingTicket = async (context) =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                    request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);

                    var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                    response.EnsureSuccessStatusCode();

                    var user = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    context.RunClaimActions(user.RootElement);
                }
            };
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
builder.Services.AddScoped<IOAuthCredentialsService, OAuthCredentialsService>();


var app = builder.Build();

// Add the user of authenticated users
app.UseAuthorization();

// Map user model
app.MapGroup("/api/auth").MapIdentityApi<AppUser>();
app.MapPost("/api/auth/Logout", async (SignInManager<AppUser> _signInManager) => {
    await _signInManager.SignOutAsync();
    return Results.Ok();
}).RequireAuthorization();

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

app.Run();
