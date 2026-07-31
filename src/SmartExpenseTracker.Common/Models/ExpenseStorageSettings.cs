namespace SmartExpenseTracker.Common.Models;

public class ExpenseStorageSettings
{
    public const string SectionName = "ExpenseStorage";
    public string FileName { get; set; } = "expenses.json";
}
