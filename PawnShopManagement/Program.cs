using Microsoft.AspNetCore.DataProtection;
using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PawnShopManagement.Data;
using PawnShopManagement.Models;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLocalization();
builder.Services.AddControllersWithViews(o => { o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); o.ModelBinderProviders.Insert(0,new InvariantNumberBinderProvider()); }).AddDataAnnotationsLocalization(o => o.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(PawnShopManagement.Resources.UiResources)));
builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o => {
    o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Account/Denied";
    o.Cookie.HttpOnly = true; o.Cookie.SameSite = SameSiteMode.Lax;
    o.ExpireTimeSpan = TimeSpan.FromHours(8);
    o.Events.OnRedirectToLogin = c => { if(c.Request.Path.StartsWithSegments("/api")) c.Response.StatusCode=401; else c.Response.Redirect(c.RedirectUri); return Task.CompletedTask; };
    o.Events.OnRedirectToAccessDenied = c => { if(c.Request.Path.StartsWithSegments("/api")) c.Response.StatusCode=403; else c.Response.Redirect(c.RedirectUri); return Task.CompletedTask; };
});
builder.Services.AddAuthorization();
var protection = builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys")));
if (OperatingSystem.IsWindows()) protection.ProtectKeysWithDpapi();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o => { o.IdleTimeout = TimeSpan.FromMinutes(30); o.Cookie.HttpOnly = true; o.Cookie.IsEssential = true; });
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<AppUser>, Microsoft.AspNetCore.Identity.PasswordHasher<AppUser>>();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.Use(async (context, next) => {
    context.Response.Headers["X-Content-Type-Options"]="nosniff";
    context.Response.Headers["X-Frame-Options"]="DENY";
    context.Response.Headers["Referrer-Policy"]="same-origin";
    await next();
});
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
var cultures = new[] { new CultureInfo("vi-VN"), new CultureInfo("en-US") };
app.UseRequestLocalization(new RequestLocalizationOptions { DefaultRequestCulture = new("vi-VN"), SupportedCultures = cultures, SupportedUICultures = cultures });
app.UseSession(); app.UseAuthentication(); app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Contracts}/{action=Index}/{id?}");
app.MapControllers();
var initialize = args.Contains("--init-db") || (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("Database:InitializeOnStartup"));
if (initialize) {
    try {
        using var scope = app.Services.CreateScope();
        await PawnShopManagement.Database.DatabaseInitializer.InitializeAsync(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(), app.Environment.ContentRootPath);
        Console.WriteLine("Database ready. First use: /Account/Setup");
    } catch (Exception ex) when (ex is Microsoft.Data.SqlClient.SqlException or InvalidOperationException) {
        Console.Error.WriteLine("Database initialization failed. Check SQL Server, DefaultConnection and database permissions.");
        Console.Error.WriteLine(ex.Message);
        Environment.ExitCode = 1;
        return;
    }
}
if (args.Contains("--init-db")) return;
app.Run();
public partial class Program { }
