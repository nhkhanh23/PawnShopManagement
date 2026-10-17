using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using PawnShopManagement.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
namespace PawnShopManagement.Database;
public static class DatabaseInitializer {
    public static async Task InitializeAsync(ApplicationDbContext db, string contentRoot) {
        if (!db.Database.IsSqlServer()) return;
        var creator = db.GetService<IRelationalDatabaseCreator>();
        if (!await creator.ExistsAsync()) {
            try {
                await creator.CreateAsync();
            } catch (SqlException ex) when (ex.Number == 1801) {
                // Another launch may have created the database after the existence check.
                // Opening it below still verifies access; never delete an existing database.
            }
        }
        await db.Database.OpenConnectionAsync();
        var script = await File.ReadAllTextAsync(Path.Combine(contentRoot, "Database", "schema.sql"));
        await using var transaction = await db.Database.BeginTransactionAsync();
        // Serialize schema changes when two development launches occur together.
        await db.Database.ExecuteSqlRawAsync("DECLARE @r int; EXEC @r = sys.sp_getapplock @Resource=N'PawnFlow.Schema', @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=15000; IF @r < 0 THROW 50002, 'Could not acquire schema lock.', 1;");
        foreach (var batch in Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)) {
            if (!string.IsNullOrWhiteSpace(batch)) await db.Database.ExecuteSqlRawAsync(batch);
        }
        await transaction.CommitAsync();
        // Verify all expected tables without fetching personal records.
        await db.AppUsers.AsNoTracking().Select(x=>x.Id).Take(1).ToListAsync();
        await db.Customers.AsNoTracking().Select(x=>x.CustomerID).Take(1).ToListAsync();
        await db.PawnContracts.AsNoTracking().Select(x=>x.ContractID).Take(1).ToListAsync();
    }
}
