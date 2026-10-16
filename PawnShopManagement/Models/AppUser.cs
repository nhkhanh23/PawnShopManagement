using System.ComponentModel.DataAnnotations;
namespace PawnShopManagement.Models;
public class AppUser {
    public int Id { get; set; }
    [Required, StringLength(50)] public string UserName { get; set; } = "";
    [Required] public string PasswordHash { get; set; } = "";
    public int FailedLoginCount { get; set; }
    public DateTime? LockoutUntil { get; set; }
    [Required, StringLength(20)] public string Role { get; set; } = "Staff";
}
public class LoginModel {
    [Required, Display(Name="Tên đăng nhập"), RegularExpression("^[a-zA-Z0-9_.-]{3,50}$", ErrorMessage="Tên đăng nhập gồm 3–50 ký tự chữ, số, dấu chấm, gạch ngang hoặc gạch dưới")] public string UserName { get; set; } = "";
    [Required, StringLength(128, MinimumLength=10, ErrorMessage="Mật khẩu từ 10 đến 128 ký tự"), DataType(DataType.Password), Display(Name="Mật khẩu")] public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
}
public class ContractListModel {
    public List<PawnContract> Items { get; set; } = [];
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? Deadline { get; set; }
    public DateTime Today { get; set; }
    public int Overdue { get; set; }
    public int DueToday { get; set; }
    public int DueSoon { get; set; }
    public int Page { get; set; }
    public int Total { get; set; }
    public int Pages => Math.Max(1,(int)Math.Ceiling(Total/10d));
    public int Active { get; set; }
    public int Redeemed { get; set; }
    public decimal Capital { get; set; }
}
