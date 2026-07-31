namespace SmartExpenseTracker.Common.DTOs.Responses;

public class MonthlyExpenseSummaryDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int Count { get; set; }
}
