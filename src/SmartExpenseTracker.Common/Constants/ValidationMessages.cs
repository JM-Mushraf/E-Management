namespace SmartExpenseTracker.Common.Constants;

public static class ValidationMessages
{
    public const string TitleRequired = "Title is required.";
    public const string TitleExceedsLength = "Title cannot exceed 50 characters.";

    public const string AmountRequired = "Amount is required.";
    public const string AmountMustBeGreaterThanZero = "Amount must be greater than zero.";

    public const string CategoryRequired = "Category is required.";
    public const string CategoryExceedsLength = "Category cannot exceed 50 characters.";

    public const string ExpenseNotFound = "Expense with specified ID was not found.";
    public const string InvalidExpenseId = "Invalid Expense ID format.";
}
