using AloMoallem.Web.Data;
using AloMoallem.Web.Models;
using AloMoallem.Web.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Email sender
builder.Services.AddSingleton<IEmailSender, FileEmailSender>();

builder.Services.AddScoped<NotificationService>();
builder.Services.AddSignalR();

// Compression
builder.Services.AddResponseCompression(o =>
{
    o.EnableForHttps = true;
    o.Providers.Add<BrotliCompressionProvider>();
    o.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.SmallestSize);

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services
    .AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = builder.Configuration.GetValue("Auth:PasswordRequiredLength", 6);
        options.Password.RequireNonAlphanumeric = builder.Configuration.GetValue("Auth:RequireNonAlphanumeric", false);
        options.User.RequireUniqueEmail = true;

        options.SignIn.RequireConfirmedAccount = builder.Configuration.GetValue("Auth:RequireConfirmedAccount", false);

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = builder.Configuration.GetValue("Auth:MaxFailedAccessAttempts", 8);
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(
            builder.Configuration.GetValue("Auth:LockoutMinutes", 10));
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(
        builder.Configuration.GetValue("Auth:CookieDays", 14));
});

// ✅ Health checks (بدون DbContextCheck)
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Security headers
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.TryAdd("X-Frame-Options", "SAMEORIGIN");
    ctx.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
    ctx.Response.Headers.TryAdd("Permissions-Policy",
        "geolocation=(), microphone=(), camera=()");
    await next();
});

app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["Cache-Control"] =
            "public,max-age=604800";
    }
});

app.UseResponseCompression();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// DB init + seed
Directory.CreateDirectory(
    Path.Combine(app.Environment.ContentRootPath, "App_Data"));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SeedData.EnsureSeededAsync(scope.ServiceProvider);
}

app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<AloMoallem.Web.Hubs.ChatHub>("/hubs/chat");
app.MapHub<AloMoallem.Web.Hubs.NotificationsHub>("/hubs/notifications");

app.Run();
