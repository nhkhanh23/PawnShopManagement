using static PawnShopManagement.Localization.UiText;
using Microsoft.AspNetCore.Mvc;
using PawnShopManagement.Models;
using System.Diagnostics;
namespace PawnShopManagement.Controllers;
public class HomeController : Controller {
    public IActionResult Index() => RedirectToAction("Index","Contracts");
    public IActionResult Privacy() => View();
    [HttpPost] public IActionResult Language(string culture,string? returnUrl) {
        if(culture is not ("vi-VN" or "en-US")) return BadRequest();
        Response.Cookies.Append(Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName, Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.MakeCookieValue(new(culture)),new CookieOptions { Expires=DateTimeOffset.UtcNow.AddYears(1),HttpOnly=true,SameSite=SameSiteMode.Lax,Secure=Request.IsHttps });
        return LocalRedirect(Url.IsLocalUrl(returnUrl)?returnUrl!:"/");
    }
    [ResponseCache(Duration=0,Location=ResponseCacheLocation.None,NoStore=true)]
    public IActionResult Error() { Response.StatusCode=500; return View(new ErrorViewModel {RequestId=Activity.Current?.Id??HttpContext.TraceIdentifier}); }
}
