namespace PawnShopManagement.Data;

public static class DatabaseConfiguration
{
    public static string ResolveConnectionString(string? configured, bool isDevelopment)
    {
        if (!string.IsNullOrWhiteSpace(configured)) return configured;

        // Local classroom setup only. A deployed app must supply its own SQL server.
        if (isDevelopment)
            return @"Server=(localdb)\mssqllocaldb;Database=PawnShopDB;Trusted_Connection=True;MultipleActiveResultSets=true";

        throw new InvalidOperationException(
            "Missing ConnectionStrings:DefaultConnection. Configure appsettings.json or the ConnectionStrings__DefaultConnection environment variable.");
    }
}
