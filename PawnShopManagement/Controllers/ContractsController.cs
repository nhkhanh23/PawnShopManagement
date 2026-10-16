using static PawnShopManagement.Localization.UiText;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PawnShopManagement.Data;
using PawnShopManagement.Models;
namespace PawnShopManagement.Controllers;
[Authorize]
public class ContractsController(ApplicationDbContext db) : Controller {
    public async Task<IActionResult> Index(string? searchString, string? statusFilter, int page=1, string? deadlineFilter=null) {
        var today=ContractDeadlines.Today;
        var q=db.PawnContracts.AsNoTracking().Include(x=>x.Customer).AsQueryable();
        if(!string.IsNullOrWhiteSpace(searchString)) { searchString=searchString.Trim(); q=q.Where(x=>x.AssetName.Contains(searchString)||x.Customer.FullName.Contains(searchString)); }
        if(!string.IsNullOrEmpty(statusFilter)) q=q.Where(x=>x.Status==statusFilter);
        q=ContractDeadlines.Filter(q,deadlineFilter,today);
        var total=await q.CountAsync(); page=Math.Clamp(page,1,Math.Max(1,(int)Math.Ceiling(total/10d)));
        var model=new ContractListModel { Search=searchString,Status=statusFilter,Deadline=deadlineFilter,Today=today,
            Overdue=await ContractDeadlines.Filter(db.PawnContracts,"Overdue",today).CountAsync(),
            DueToday=await ContractDeadlines.Filter(db.PawnContracts,"DueToday",today).CountAsync(),
            DueSoon=await ContractDeadlines.Filter(db.PawnContracts,"DueSoon",today).CountAsync(),Page=page,Total=total,
            Items=await q.OrderByDescending(x=>x.ContractID).Skip((page-1)*10).Take(10).ToListAsync(),
            Active=await db.PawnContracts.CountAsync(x=>x.Status=="DangCam"), Redeemed=await db.PawnContracts.CountAsync(x=>x.Status=="DaChuoc"),
            Capital=await db.PawnContracts.Where(x=>x.Status=="DangCam").SumAsync(x=>(decimal?)x.LoanAmount)??0 };
        HttpContext.Session.SetString("LastSearch",searchString??"");
        if(Request.Headers["X-Requested-With"]=="XMLHttpRequest") return PartialView("_Results",model);
        return View(model);
    }
    private async Task Customers(int? id=null) => ViewBag.CustomerID=new SelectList(await db.Customers.AsNoTracking().OrderBy(x=>x.FullName).ToListAsync(),"CustomerID","FullName",id);
    [HttpGet] public async Task<IActionResult> Create() { await Customers(); return View("Form",new PawnContract()); }
    [HttpPost] public async Task<IActionResult> Create([Bind("CustomerID,AssetName,AssetDetails,LoanAmount,InterestRate,StartDate,TermDays,Status")] PawnContract model) {
        await Validate(model);
        if(ModelState.IsValid) { db.Add(model); await db.SaveChangesAsync(); TempData["Success"]=T("Đã tạo hợp đồng thành công."); return RedirectToAction(nameof(Index)); }
        await Customers(model.CustomerID); return View("Form",model);
    }
    [HttpGet] public async Task<IActionResult> Edit(int id) { var m=await db.PawnContracts.FindAsync(id); if(m==null) return NotFound(); await Customers(m.CustomerID); return View("Form",m); }
    [HttpPost] public async Task<IActionResult> Edit(int id,[Bind("ContractID,CustomerID,AssetName,AssetDetails,LoanAmount,InterestRate,StartDate,TermDays,Status")] PawnContract model) {
        if(id!=model.ContractID) return BadRequest();
        var existing=await db.PawnContracts.FindAsync(id); if(existing==null) return NotFound();
        await Validate(model);
        if(ModelState.IsValid) { db.Entry(existing).CurrentValues.SetValues(model); await db.SaveChangesAsync(); TempData["Success"]=T("Đã cập nhật hợp đồng."); return RedirectToAction(nameof(Index)); }
        await Customers(model.CustomerID); return View("Form",model);
    }
    private async Task Validate(PawnContract m) {
        if(!await db.Customers.AnyAsync(x=>x.CustomerID==m.CustomerID)) ModelState.AddModelError("CustomerID",T("Khách hàng không tồn tại."));
        if(m.StartDate.Date>ContractDeadlines.Today || m.StartDate.Year<2000) ModelState.AddModelError("StartDate",T("Ngày cầm phải từ năm 2000 đến hôm nay."));
    }
    [Authorize(Roles="Admin"),HttpPost] public async Task<IActionResult> Delete(int id) {
        var m=await db.PawnContracts.FindAsync(id); if(m==null) return NotFound(); db.Remove(m); await db.SaveChangesAsync(); TempData["Success"]=T("Đã xóa hợp đồng."); return RedirectToAction(nameof(Index));
    }
}
