using System.ComponentModel.DataAnnotations;

namespace FalconProgrammer.ViewModel;

public class CcNoRangeItem : DataGridItem {
  private int? _end;
  private int? _start;

  public CcNoRangeItem(bool isAdditionItem) : base(isAdditionItem) { }

  [Required]
  [Range(1, 127)]
  [CustomValidation(typeof(CcNoRangeItem), nameof(ValidateStart))]
  public int? Start {
    get => _start;
    set => SetProperty(ref _start, value, true);
  }

  [Required]
  [Range(1, 127)]
  [CustomValidation(typeof(CcNoRangeItem), nameof(ValidateEnd))]
  public int? End {
    get => _end;
    set => SetProperty(ref _end, value, true);
  }

  public static ValidationResult ValidateStart(int start, ValidationContext context) {
    var instance = (CcNoRangeItem)context.ObjectInstance;
    bool isValid = start <= instance.End || instance.End == null;
    return isValid
      ? ValidationResult.Success!
      : new ValidationResult("Start must be <= End.", [nameof(Start)]);
  }

  public static ValidationResult ValidateEnd(int end, ValidationContext context) {
    var instance = (CcNoRangeItem)context.ObjectInstance;
    bool isValid = end >= instance.Start;
    return isValid
      ? ValidationResult.Success!
      : new ValidationResult("End must be >= Start.", [nameof(End)]);
  }
}