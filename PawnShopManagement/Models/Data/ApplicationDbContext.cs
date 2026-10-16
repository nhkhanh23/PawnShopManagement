using Microsoft.EntityFrameworkCore;
using PawnShopManagement.Models;
namespace PawnShopManagement.Data;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options) {
    public DbSet<PawnContract> PawnContracts => Set<PawnContract>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    protected override void OnModelCreating(ModelBuilder b) {
        // Legacy contracts may have no agreed term recorded; MVC still requires
        // a term on new/edited contracts through the validation annotation.
        b.Entity<PawnContract>().Property(x=>x.TermDays).IsRequired(false);
        b.Entity<PawnContract>().Property(x=>x.LoanAmount).HasPrecision(18,2);
        b.Entity<PawnContract>().HasOne(x=>x.Customer).WithMany().HasForeignKey(x=>x.CustomerID).OnDelete(DeleteBehavior.Restrict);
        b.Entity<AppUser>().HasIndex(x=>x.UserName).IsUnique();
        b.Entity<Customer>().HasIndex(x=>x.IdentityCard).IsUnique();
    }
}
