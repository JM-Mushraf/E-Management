namespace SmartExpenseTracker.Common.DTOs.Responses;

public class CategoryExpenseSummaryDto
{
    public string? Category { get; set; }
    public decimal TotalAmount { get; set; }
    public int Count { get; set; }
}
