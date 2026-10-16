using static PawnShopManagement.Localization.UiText;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PawnShopManagement.Data;
using PawnShopManagement.Models;
namespace PawnShopManagement.Controllers.Api;
[Authorize, ApiController, Route("api/contracts")]
public class ContractsApiController(ApplicationDbContext db) : ControllerBase {
    [HttpGet] public async Task<IActionResult> Get(string? search,string? status,int page=1,string? deadline=null) {
        var q=db.PawnContracts.AsNoTracking().AsQueryable();
        if(!string.IsNullOrWhiteSpace(search)) q=q.Where(x=>x.AssetName.Contains(search)||x.Customer.FullName.Contains(search));
        if(!string.IsNullOrEmpty(status)) q=q.Where(x=>x.Status==status);
        q=ContractDeadlines.Filter(q,deadline,ContractDeadlines.Today);
        var total=await q.CountAsync(); page=Math.Clamp(page,1,Math.Max(1,(int)Math.Ceiling(total/10d)));
        return Ok(new {total,page,pageSize=10,items=await q.OrderByDescending(x=>x.ContractID).Skip((page-1)*10).Take(10).Select(x=>new {x.ContractID,x.CustomerID,CustomerName=x.Customer.FullName,x.AssetName,x.AssetDetails,x.LoanAmount,x.InterestRate,x.StartDate,x.TermDays,DueDate=x.TermDays==null?(DateTime?)null:x.StartDate.Date.AddDays(x.TermDays.Value),x.Status}).ToListAsync()});
    }
    [HttpGet("{id:int}")] public async Task<IActionResult> GetById(int id) {
        var m=await db.PawnContracts.AsNoTracking().Where(x=>x.ContractID==id).Select(x=>new {x.ContractID,x.CustomerID,CustomerName=x.Customer.FullName,x.AssetName,x.AssetDetails,x.LoanAmount,x.InterestRate,x.StartDate,x.TermDays,DueDate=x.TermDays==null?(DateTime?)null:x.StartDate.Date.AddDays(x.TermDays.Value),x.Status}).SingleOrDefaultAsync(); return m==null?NotFound():Ok(m);
    }
    [HttpPost] public async Task<IActionResult> Create(PawnContract m) {
        m.ContractID=0; await Validate(m); if(!ModelState.IsValid) return ValidationProblem(ModelState);
        db.Add(m); await db.SaveChangesAsync(); return CreatedAtAction(nameof(GetById),new{id=m.ContractID},new {m.ContractID});
    }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id,PawnContract m) {
        if(id!=m.ContractID) return BadRequest(); var old=await db.PawnContracts.FindAsync(id); if(old==null) return NotFound();
        await Validate(m); if(!ModelState.IsValid) return ValidationProblem(ModelState);
        db.Entry(old).CurrentValues.SetValues(m); await db.SaveChangesAsync(); return NoContent();
    }
    [Authorize(Roles="Admin"),HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id) { var m=await db.PawnContracts.FindAsync(id); if(m==null) return NotFound(); db.Remove(m); await db.SaveChangesAsync(); return NoContent(); }
    private async Task Validate(PawnContract m) {
        if(!await db.Customers.AnyAsync(x=>x.CustomerID==m.CustomerID)) ModelState.AddModelError("CustomerID",T("Khách hàng không tồn tại."));
        if(m.StartDate.Date>ContractDeadlines.Today||m.StartDate.Year<2000) ModelState.AddModelError("StartDate",T("Ngày cầm không hợp lệ."));
    }
}
