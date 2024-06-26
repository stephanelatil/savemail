using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddAuthorization();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//insert DBContext
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<ApplicationDBContext>(opt =>
        {
            opt.UseInMemoryDatabase("SaveMailDev");
        });
}
else
{
    builder.Services.AddDbContext<ApplicationDBContext>(opt =>
        {
            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            if (connectionString.Length < "localhost".Length)
            {
                //TODO: use env variables here
            }
            opt.UseNpgsql(connectionString);
        });
}
//Setup SendGrid
//string a = builder.Configuration.GetRequiredSection("SendGrid")['SendGridKey'];

builder.Services.AddProblemDetails();
// Add User auth
builder.Services.AddIdentityApiEndpoints<AppUser>()
    .AddEntityFrameworkStores<ApplicationDBContext>().AddDefaultTokenProviders();

//Removes need for email confimation (no email server yet)
builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Lockout.AllowedForNewUsers=false;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(20);
    options.Lockout.MaxFailedAccessAttempts = 5;
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

var app = builder.Build();

// Add the user of authenticated users
app.UseAuthorization();

// Map user model
app.MapGroup("/api/auth").MapIdentityApi<AppUser>();

//In case of 4xx error so it doesn't leak info
app.UseStatusCodePages(async context =>
{
    context.HttpContext.Response.ContentType = "application/json";
    var result = $"{{'message':'{context.HttpContext.Response.StatusCode}'}}";
    await context.HttpContext.Response.WriteAsync(result);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
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

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
