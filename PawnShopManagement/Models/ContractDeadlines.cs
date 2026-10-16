namespace PawnShopManagement.Models;
public static class ContractDeadlines {
    private static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows()?"SE Asia Standard Time":"Asia/Ho_Chi_Minh");
    public static DateTime Today => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,Zone).Date;
    public static IQueryable<PawnContract> Filter(IQueryable<PawnContract> query,string? deadline,DateTime today) {
        var upcoming=today.AddDays(3);
        return deadline switch {
            "Overdue" => query.Where(x=>x.Status=="DangCam" && x.TermDays!=null && x.StartDate.Date.AddDays(x.TermDays.Value)<today),
            "DueToday" => query.Where(x=>x.Status=="DangCam" && x.TermDays!=null && x.StartDate.Date.AddDays(x.TermDays.Value)==today),
            "DueSoon" => query.Where(x=>x.Status=="DangCam" && x.TermDays!=null && x.StartDate.Date.AddDays(x.TermDays.Value)>today && x.StartDate.Date.AddDays(x.TermDays.Value)<=upcoming),
            _ => query
        };
    }
}
