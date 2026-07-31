namespace SmartExpenseTracker.Common.Models;

public class Expense
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Title { get; set; }
    public decimal Amount { get; set; }
    public string? Category { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
}
