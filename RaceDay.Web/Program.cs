using Microsoft.AspNetCore.Authentication.Cookies;
using RaceDay.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- MVC ----------
builder.Services.AddControllersWithViews();

// ---------- Session ----------
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(120);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

// ---------- Cookie Authentication ----------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Auth/Login";
        o.LogoutPath = "/Auth/Logout";
        o.AccessDeniedPath = "/Auth/AccessDenied";
        o.ExpireTimeSpan = TimeSpan.FromMinutes(120);
    });

// ---------- Services ----------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<RaceDayApiClient>();

var app = builder.Build();

// ---------- Pipeline ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();