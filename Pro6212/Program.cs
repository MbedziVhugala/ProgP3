using Microsoft.AspNetCore.Authentication.Cookies;
using Prog6212.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC setup
builder.Services.AddControllersWithViews();

// ✅ Add this line to register your service
builder.Services.AddSingleton<IDataService, DataService>();
builder.Services.AddSingleton<IDataService, FakeDataService>();


// Authentication setup
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();
