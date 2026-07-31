using System.ComponentModel.DataAnnotations;
using SmartExpenseTracker.Common.Constants;

namespace SmartExpenseTracker.Common.DTOs.Requests;

public class CreateExpenseRequestDto
{
    [Required(ErrorMessage = ValidationMessages.TitleRequired)]
    [StringLength(50, ErrorMessage = ValidationMessages.TitleExceedsLength)]
    public string? Title { get; set; }

    [Required(ErrorMessage = ValidationMessages.AmountRequired)]
    [Range(0.01, 1000000000.0, ErrorMessage = ValidationMessages.AmountMustBeGreaterThanZero)]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = ValidationMessages.CategoryRequired)]
    [StringLength(50, ErrorMessage = ValidationMessages.CategoryExceedsLength)]
    public string? Category { get; set; }

    public DateTime? Date { get; set; }
}
