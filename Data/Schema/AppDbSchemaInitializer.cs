using Microsoft.EntityFrameworkCore;

namespace BTLWEB.Data;

public static class AppDbSchemaInitializer
{
    public static Task EnsureAuthSchemaAsync(AppDbContext dbContext)
    {
        return EnsureSchemaAsync(dbContext);
    }

    public static async Task EnsureSchemaAsync(AppDbContext dbContext)
    {
        var sqlFilePath = Path.Combine(AppContext.BaseDirectory, "Database", "init-btlweb.sql");

        if (!File.Exists(sqlFilePath))
        {
            throw new FileNotFoundException(
                $"Khong tim thay file schema: {sqlFilePath}. " +
                "Dam bao file Database/init-btlweb.sql duoc copy vao thu muc output.");
        }

        var sqlContent = await File.ReadAllTextAsync(sqlFilePath);
        foreach (var batch in SplitBatches(sqlContent))
        {
            await dbContext.Database.ExecuteSqlRawAsync(batch);
        }
    }

    private static List<string> SplitBatches(string sql)
    {
        var batches = new List<string>();
        var lines = sql.Split('\n');
        var currentBatch = new List<string>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Equals("GO", StringComparison.OrdinalIgnoreCase))
            {
                AddBatchIfNotEmpty(batches, currentBatch);
                currentBatch.Clear();
            }
            else
            {
                currentBatch.Add(line);
            }
        }

        AddBatchIfNotEmpty(batches, currentBatch);
        return batches;
    }

    private static void AddBatchIfNotEmpty(List<string> batches, List<string> currentBatch)
    {
        var batch = string.Join('\n', currentBatch).Trim();
        if (!string.IsNullOrEmpty(batch))
        {
            batches.Add(batch);
        }
    }
}
