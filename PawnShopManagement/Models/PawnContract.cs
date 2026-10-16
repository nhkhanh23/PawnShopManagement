using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace PawnShopManagement.Models;
[Table("PawnContracts")]
public class PawnContract {
    [Key] public int ContractID { get; set; }
    [Range(1,int.MaxValue,ErrorMessage="Vui lòng chọn khách hàng")] [Display(Name="Khách hàng")] public int CustomerID { get; set; }
    [ValidateNever, System.Text.Json.Serialization.JsonIgnore] public Customer Customer { get; set; } = null!;
    [Required(ErrorMessage="Vui lòng nhập tên tài sản")] [StringLength(150)] [Display(Name="Tên tài sản")] public string AssetName { get; set; } = "";
    [StringLength(2000)] [Display(Name="Mô tả tài sản")] public string? AssetDetails { get; set; }
    [Range(typeof(decimal),"500000","1000000000",ErrorMessage="Số tiền vay từ 500.000 đến 1.000.000.000 đồng")] [Display(Name="Số tiền vay (VNĐ)")] public decimal LoanAmount { get; set; }
    [Range(0,100,ErrorMessage="Lãi suất phải từ 0 đến 100%")] [Display(Name="Lãi suất (%/tháng)")] public float InterestRate { get; set; } = 1;
    [DataType(DataType.Date)] [Display(Name="Ngày cầm")] public DateTime StartDate { get; set; } = ContractDeadlines.Today;
    [Required(ErrorMessage="Vui lòng nhập thời hạn cầm")]
    [Range(1,3650,ErrorMessage="Thời hạn cầm phải từ 1 đến 3650 ngày")]
    [Display(Name="Thời hạn cầm (ngày)")]
    public int? TermDays { get; set; } = 30;

    [NotMapped] public DateTime? DueDate => TermDays is >=1 and <=3650 && StartDate.Date <= DateTime.MaxValue.Date.AddDays(-TermDays.Value)
        ? StartDate.Date.AddDays(TermDays.Value) : null;
    public string GetDeadlineState(DateTime today) {
        if(Status != "DangCam") return "Closed";
        if(DueDate is not DateTime due) return "Unknown";
        var days=(due-today.Date).Days;
        return days<0?"Overdue":days==0?"DueToday":days<=3?"DueSoon":"OnTime";
    }
    [Required] [RegularExpression("^(DangCam|DaChuoc|DaThanhLy)$",ErrorMessage="Trạng thái không hợp lệ")] [Display(Name="Trạng thái")] public string Status { get; set; } = "DangCam";
}
[Table("Customers")]
public class Customer {
    [Key] public int CustomerID { get; set; }
    [Required(ErrorMessage="Vui lòng nhập họ tên")] [StringLength(120)] [Display(Name="Họ và tên")] public string FullName { get; set; } = "";
    [Required(ErrorMessage="Vui lòng nhập CCCD")] [RegularExpression(@"^(\d{9}|\d{12})$",ErrorMessage="CCCD/CMND gồm 9 hoặc 12 chữ số")] [Display(Name="CCCD / CMND")] public string IdentityCard { get; set; } = "";
    [Required] [RegularExpression(@"^0\d{9}$",ErrorMessage="Số điện thoại gồm 10 chữ số, bắt đầu bằng 0")] [Display(Name="Số điện thoại")] public string Phone { get; set; } = "";
    [Required] [StringLength(250)] [Display(Name="Địa chỉ")] public string Address { get; set; } = "";
}
