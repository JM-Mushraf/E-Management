namespace SmartExpenseTracker.Common.DTOs.Responses;

public class ExpenseResponseDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public decimal Amount { get; set; }
    public string? Category { get; set; }
    public DateTime Date { get; set; }
}
