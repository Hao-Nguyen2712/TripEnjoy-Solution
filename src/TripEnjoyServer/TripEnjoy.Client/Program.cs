using Microsoft.AspNetCore.Authentication.Cookies;
using TripEnjoy.Client.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthenticationDelegatingHandler>();

// This client is used by the AuthenticationDelegatingHandler to refresh the token.
// It does NOT have the handler itself, to avoid an infinite loop.
builder.Services.AddHttpClient("AuthApiClient");

// This is the main client for calling protected APIs.
// It has the handler that will automatically refresh tokens.
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7199");
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        options.LoginPath = "/authen/sign-in";
        options.LogoutPath = "/authen/sign-out";
        options.AccessDeniedPath = "/authen/access-denied"; // Add path for access denied
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensure cookies are only sent over HTTPS
        options.Cookie.SameSite = SameSiteMode.Lax; // Changed from Strict to Lax to allow cross-site redirects for OAuth
        options.Cookie.Name = "TripEnjoy.Auth"; // Custom name for the cookie
    });


var app = builder.Build();


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
