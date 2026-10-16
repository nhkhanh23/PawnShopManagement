using static PawnShopManagement.Localization.UiText;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PawnShopManagement.Data;
using PawnShopManagement.Models;
namespace PawnShopManagement.Controllers;
[Authorize]
public class CustomersController(ApplicationDbContext db) : Controller {
    public async Task<IActionResult> Index(string? search,int page=1) {
        var q=db.Customers.AsNoTracking().AsQueryable();
        if(!string.IsNullOrWhiteSpace(search)) q=q.Where(x=>x.FullName.Contains(search)||x.Phone.Contains(search)||x.IdentityCard.Contains(search));
        var count=await q.CountAsync(); var pages=Math.Max(1,(int)Math.Ceiling(count/10d)); page=Math.Clamp(page,1,pages);
        ViewBag.Search=search; ViewBag.Page=page; ViewBag.Pages=pages; ViewBag.Total=count;
        return View(await q.OrderByDescending(x=>x.CustomerID).Skip((page-1)*10).Take(10).ToListAsync());
    }
    [HttpGet] public IActionResult Create() => View("Form",new Customer());
    [HttpPost] public async Task<IActionResult> Create([Bind("FullName,IdentityCard,Phone,Address")] Customer m) {
        await Validate(m);
        if(!ModelState.IsValid) return View("Form",m);
        db.Add(m); await db.SaveChangesAsync(); TempData["Success"]=T("Đã thêm khách hàng."); return RedirectToAction(nameof(Index));
    }
    [HttpGet] public async Task<IActionResult> Edit(int id) { var m=await db.Customers.FindAsync(id); return m==null?NotFound():View("Form",m); }
    [HttpPost] public async Task<IActionResult> Edit(int id,[Bind("CustomerID,FullName,IdentityCard,Phone,Address")] Customer m) {
        if(id!=m.CustomerID) return BadRequest(); var old=await db.Customers.FindAsync(id); if(old==null) return NotFound(); await Validate(m);
        if(!ModelState.IsValid) return View("Form",m); db.Entry(old).CurrentValues.SetValues(m); await db.SaveChangesAsync(); TempData["Success"]=T("Đã cập nhật khách hàng."); return RedirectToAction(nameof(Index));
    }
    private async Task Validate(Customer m) { if(await db.Customers.AnyAsync(x=>x.IdentityCard==m.IdentityCard&&x.CustomerID!=m.CustomerID)) ModelState.AddModelError("IdentityCard",T("CCCD đã được đăng ký.")); }
    [Authorize(Roles="Admin"),HttpPost] public async Task<IActionResult> Delete(int id) {
        var m=await db.Customers.FindAsync(id); if(m==null) return NotFound();
        if(await db.PawnContracts.AnyAsync(x=>x.CustomerID==id)) TempData["Error"]=T("Không thể xóa khách hàng đang có hợp đồng.");
        else { db.Remove(m); await db.SaveChangesAsync(); TempData["Success"]=T("Đã xóa khách hàng."); }
        return RedirectToAction(nameof(Index));
    }
}
