using Microsoft.EntityFrameworkCore;
using PawnShopManagement.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
namespace PawnShopManagement.Database;
public static class DatabaseInitializer {
    public static async Task InitializeAsync(ApplicationDbContext db, string contentRoot) {
        if (!db.Database.IsSqlServer()) return;
        Console.WriteLine("[DB-FIX-02] Checking database access...");
        var original = new SqlConnectionStringBuilder(db.Database.GetConnectionString());
        if (string.IsNullOrWhiteSpace(original.InitialCatalog))
            throw new InvalidOperationException("DefaultConnection must specify Database / Initial Catalog.");
        // First try the actual database. An existing accessible database needs no CREATE permission.
        var createNeeded = false;
        await using (var probe = new SqlConnection(original.ConnectionString)) {
            try { await OpenWithLocalDbWarmupAsync(probe); }
            catch (SqlException ex) when (ex.Number == 4060) { createNeeded = true; }
        }
        if (createNeeded) {
            var masterSettings = new SqlConnectionStringBuilder(original.ConnectionString) {
                InitialCatalog = "master", Pooling = false
            };
            await using var master = new SqlConnection(masterSettings.ConnectionString);
            await OpenWithLocalDbWarmupAsync(master);
            await using var check = master.CreateCommand();
            check.CommandText = "SELECT state_desc FROM sys.databases WHERE name = @name";
            check.Parameters.AddWithValue("@name", original.InitialCatalog);
            var state = await check.ExecuteScalarAsync();
            if (state is not null) {
                throw new InvalidOperationException($"Database '{original.InitialCatalog}' exists (state: {state}) but this Windows/SQL login cannot open it. Check database state and login permissions; data has not been deleted.");
            }
            await using var create = master.CreateCommand();
            create.CommandText = "CREATE DATABASE [" + original.InitialCatalog.Replace("]", "]]") + "]";
            create.CommandTimeout = 60;
            try { await create.ExecuteNonQueryAsync(); }
            catch (SqlException ex) when (ex.Number == 1801) {
                // A concurrent initializer created it. Verify access below instead of recreating it.
            }
            // Remove the failed-open pool's blocking period before verifying the new database.
            using var poolConnection = new SqlConnection(original.ConnectionString);
            SqlConnection.ClearPool(poolConnection);
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
    private static async Task OpenWithLocalDbWarmupAsync(SqlConnection connection) {
        // The automatic LocalDB instance can take a few seconds to start on a new Windows profile.
        for (var attempt = 0; ; attempt++) {
            try { await connection.OpenAsync(); return; }
            catch (SqlException ex) when (attempt < 2
                && connection.DataSource.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase)
                && (ex.Number == 50 || ex.Number == -2)) {
                Console.WriteLine("Waiting for SQL Server LocalDB to start...");
                SqlConnection.ClearPool(connection);
                await Task.Delay(2000);
            }
        }
    }
}
