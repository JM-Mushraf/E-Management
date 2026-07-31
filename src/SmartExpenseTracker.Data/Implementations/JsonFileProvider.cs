using System.Text.Json;
using SmartExpenseTracker.Common.Models;
using SmartExpenseTracker.Data.Abstractions;

namespace SmartExpenseTracker.Data.Implementations;

public class JsonFileProvider : IJsonFileProvider
{
    private static readonly SemaphoreSlim Lock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public string GetFilePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, fileName);
    }

    public async Task<List<Expense>> ReadExpensesAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(fileName);

        await Lock.WaitAsync(cancellationToken);
        try
        {
            EnsureFileExists(filePath);

            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (fileStream.Length == 0)
            {
                return new List<Expense>();
            }

            try
            {
                var expenses = await JsonSerializer.DeserializeAsync<List<Expense>>(fileStream, JsonOptions, cancellationToken);
                return expenses ?? new List<Expense>();
            }
            catch (JsonException)
            {
                // Edge Case: Handle malformed or corrupt JSON file gracefully by returning an empty list
                return new List<Expense>();
            }
        }
        finally
        {
            Lock.Release();
        }
    }

    public async Task WriteExpensesAsync(string fileName, List<Expense> expenses, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(fileName);

        await Lock.WaitAsync(cancellationToken);
        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(fileStream, expenses, JsonOptions, cancellationToken);
        }
        finally
        {
            Lock.Release();
        }
    }

    private static void EnsureFileExists(string filePath)
    {
        if (!File.Exists(filePath))
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, "[]");
        }
    }
}
