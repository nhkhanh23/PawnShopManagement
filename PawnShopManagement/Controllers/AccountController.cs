using static PawnShopManagement.Localization.UiText;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PawnShopManagement.Data;
using PawnShopManagement.Models;
namespace PawnShopManagement.Controllers;
public class AccountController(ApplicationDbContext db, IPasswordHasher<AppUser> hasher, IWebHostEnvironment env) : Controller {
    [HttpGet] public async Task<IActionResult> Start() {
        if (env.IsDevelopment() && !await db.AppUsers.AnyAsync()) return RedirectToAction(nameof(Setup));
        return RedirectToAction(nameof(Login));
    }
    [HttpGet] public IActionResult Login() => View(new LoginModel());
    [HttpPost] public async Task<IActionResult> Login(LoginModel input, string? returnUrl) {
        if(!ModelState.IsValid) return View(input);
        var attempts = HttpContext.Session.GetInt32("attempts") ?? 0;
        var until = HttpContext.Session.GetString("lockedUntil");
        if(DateTime.TryParse(until, out var locked) && locked > DateTime.UtcNow) { ModelState.AddModelError("", T("Thử lại sau 5 phút.")); return View(input); }
        if(locked <= DateTime.UtcNow && until != null) attempts = 0;
        var user = await db.AppUsers.SingleOrDefaultAsync(x=>x.UserName==input.UserName);
        if(user?.LockoutUntil > DateTime.UtcNow) { ModelState.AddModelError("", T("Tài khoản tạm khóa. Vui lòng thử lại sau 5 phút.")); return View(input); }
        if(user == null || hasher.VerifyHashedPassword(user,user.PasswordHash,input.Password)==PasswordVerificationResult.Failed) {
            if(user != null) {
                user.FailedLoginCount = user.LockoutUntil.HasValue && user.LockoutUntil<=DateTime.UtcNow ? 1 : user.FailedLoginCount+1;
                user.LockoutUntil=user.FailedLoginCount>=5?DateTime.UtcNow.AddMinutes(5):null;
                await db.SaveChangesAsync();
            }
            HttpContext.Session.SetInt32("attempts", attempts+1);
            if(attempts+1>=5) HttpContext.Session.SetString("lockedUntil",DateTime.UtcNow.AddMinutes(5).ToString("O"));
            ModelState.AddModelError("", T("Tên đăng nhập hoặc mật khẩu không đúng.")); return View(input);
        }
        user.FailedLoginCount=0; user.LockoutUntil=null; await db.SaveChangesAsync();
        HttpContext.Session.Remove("attempts"); HttpContext.Session.Remove("lockedUntil");
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(new[] {new Claim(ClaimTypes.Name,user.UserName),new Claim(ClaimTypes.Role,user.Role)},CookieAuthenticationDefaults.AuthenticationScheme)), new AuthenticationProperties { IsPersistent=input.RememberMe });
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/Contracts");
    }
    [HttpGet] public async Task<IActionResult> Setup() {
        if(!env.IsDevelopment() || await db.AppUsers.AnyAsync()) return NotFound();
        return View("Login",new LoginModel());
    }
    [HttpPost] public async Task<IActionResult> Setup(LoginModel input) {
        if(!env.IsDevelopment()) return NotFound();
        if(!ModelState.IsValid) return View("Login",input);
        // Serializable transaction prevents two simultaneous first-admin registrations.
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        if(await db.AppUsers.AnyAsync()) return NotFound();
        var user=new AppUser { UserName=input.UserName,Role="Admin" };
        user.PasswordHash=hasher.HashPassword(user,input.Password); db.Add(user); await db.SaveChangesAsync(); await tx.CommitAsync();
        TempData["Success"]=T("Đã tạo quản trị viên. Hãy đăng nhập."); return RedirectToAction(nameof(Login));
    }
    [Authorize, HttpPost] public async Task<IActionResult> Logout() { await HttpContext.SignOutAsync(); HttpContext.Session.Clear(); return RedirectToAction(nameof(Login)); }
    [Authorize,HttpGet] public IActionResult Token([FromServices] Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery) => Json(new { requestToken=antiforgery.GetAndStoreTokens(HttpContext).RequestToken });
    public IActionResult Denied() { Response.StatusCode=403; return View(); }
    [Authorize(Roles="Admin"),HttpGet] public IActionResult CreateStaff() => View("Login",new LoginModel());
    [Authorize(Roles="Admin"),HttpPost] public async Task<IActionResult> CreateStaff(LoginModel input) {
        if(await db.AppUsers.AnyAsync(x=>x.UserName==input.UserName)) ModelState.AddModelError("UserName",T("Tên đăng nhập đã tồn tại."));
        if(!ModelState.IsValid) return View("Login",input);
        var u = new AppUser { UserName=input.UserName, Role="Staff" }; u.PasswordHash=hasher.HashPassword(u,input.Password); db.Add(u); await db.SaveChangesAsync(); TempData["Success"]=T("Đã tạo tài khoản nhân viên."); return RedirectToAction("Index","Contracts");
    }
}
