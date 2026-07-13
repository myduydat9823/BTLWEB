using Microsoft.EntityFrameworkCore;

namespace BTLWEB.Data;

public static class AppDbSchemaInitializer
{
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
        var batches = SplitBatches(sqlContent);

        foreach (var batch in batches)
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
                var batch = string.Join('\n', currentBatch).Trim();
                if (!string.IsNullOrEmpty(batch))
                {
                    batches.Add(batch);
                }
                currentBatch.Clear();
            }
            else
            {
                currentBatch.Add(line);
            }
        }

        var remaining = string.Join('\n', currentBatch).Trim();
        if (!string.IsNullOrEmpty(remaining))
        {
            batches.Add(remaining);
        }

        return batches;
    }
}