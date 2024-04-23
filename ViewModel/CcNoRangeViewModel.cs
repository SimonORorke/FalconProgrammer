using System.ComponentModel.DataAnnotations;

namespace FalconProgrammer.ViewModel;

public class CcNoRangeViewModel : DataGridItem {
  private int? _end;
  private int? _start;

  public CcNoRangeViewModel(Action appendAdditionItem,
    Action onItemChanged,
    Action<DataGridItem> removeItem,
    bool isAdditionItem,
    Action<DataGridItem> cutItem,
    Action<DataGridItem> pasteBeforeItem) : base(appendAdditionItem,
    onItemChanged, removeItem, isAdditionItem, cutItem, pasteBeforeItem) { }

  [Required]
  [Range(0, 127)]
  [CustomValidation(typeof(CcNoRangeViewModel), nameof(ValidateStart))]
  public int? Start {
    get => _start;
    set => SetProperty(ref _start, value, true);
  }

  [Required]
  [Range(0, 127)]
  [CustomValidation(typeof(CcNoRangeViewModel), nameof(ValidateEnd))]
  public int? End {
    get => _end;
    set => SetProperty(ref _end, value, true);
  }

  public static ValidationResult ValidateStart(int start, ValidationContext context) {
    var instance = (CcNoRangeViewModel)context.ObjectInstance;
    bool isValid = start <= instance.End || instance.End == null;
    return isValid
      ? ValidationResult.Success!
      : new ValidationResult("Start must be <= End.", [nameof(Start)]);
  }

  public static ValidationResult ValidateEnd(int end, ValidationContext context) {
    var instance = (CcNoRangeViewModel)context.ObjectInstance;
    bool isValid = end >= instance.Start;
    return isValid
      ? ValidationResult.Success!
      : new ValidationResult("End must be >= Start.", [nameof(End)]);
  }
}