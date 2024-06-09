using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class CcNoRangeCollection : DataGridItemCollectionBase<CcNoRangeItem> {
  public CcNoRangeCollection(string rangeType,
    IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dispatcherService) {
    RangeType = rangeType;
    DialogService = dialogService;
  }

  private IDialogService DialogService { get; }

  internal override bool HasBeenChanged =>
    base.HasBeenChanged
    // Force validation of errors read from settings, even if nothing has changed.
    || GetErrorMessage() != string.Empty;

  /// <summary>
  ///   Gets the range items excluding the addition item.
  /// </summary>
  private IReadOnlyCollection<CcNoRangeItem> Ranges => (
    from range in this
    where !range.IsAdditionItem
    select range).ToList();

  private string RangeType { get; }
  private List<Settings.IntegerRange> SettingsRanges { get; set; } = null!;

  protected override void AppendAdditionItem() {
    AddItem();
  }

  private void AddItem(int? start = null, int? end = null) {
    AddItem(new CcNoRangeItem(IsAddingAdditionItem) {
      Start = start,
      End = end
    });
  }

  private void CheckRangeForOverlap(CcNoRangeItem range) {
    var otherRanges =
      from otherRange in Ranges
      where otherRange != range
      select otherRange;
    var overlappingRange = (
      from otherRange in otherRanges
      where otherRange.Start <= range.End && range.Start <= otherRange.End
      select otherRange).FirstOrDefault();
    if (overlappingRange != null) {
      throw new ApplicationException(
        $"{RangeType} CC No range {range.Start} to {range.End} " +
        $"overlaps with range {overlappingRange.Start} to {overlappingRange.End}.");
    }
  }

  protected override void CutItem(DataGridItemBase itemToCut) {
    CutItemTyped((CcNoRangeItem)itemToCut);
  }

  /// <summary>
  ///   Returns an error message if invalid, an empty string if valid.
  /// </summary>
  private string GetErrorMessage() {
    if (Ranges.Count == 0) {
      return string.Empty;
    }
    var errorMessageWriter = new StringWriter();
    foreach (var range in Ranges) {
      if (range.HasErrors) {
        errorMessageWriter.WriteLine(
          $"{RangeType} CC No range {range.Start} to {range.End} " +
          "has a validation error.");
      }
    }
    string errorMessage = errorMessageWriter.ToString().TrimEnd('\r', '\n');
    if (errorMessage == string.Empty) {
      // No ranges have internal consistency errors. So check for range overlaps.
      foreach (var range in Ranges) {
        try {
          CheckRangeForOverlap(range);
        } catch (ApplicationException exception) {
          errorMessage = exception.Message;
        }
      }
    }
    return errorMessage;
  }

  internal void Populate(List<Settings.IntegerRange> settingsRanges) {
    IsPopulating = true;
    SettingsRanges = settingsRanges;
    Clear();
    foreach (var settingsRange in SettingsRanges) {
      AddItem(settingsRange.Start, settingsRange.End);
    }
    IsPopulating = false;
  }

  protected override void PasteBeforeItem(DataGridItemBase itemBeforeWhichToPaste) {
    PasteBeforeItemTyped((CcNoRangeItem)itemBeforeWhichToPaste);
  }

  protected override void RemoveItem(DataGridItemBase itemToRemove) {
    RemoveItemTyped((CcNoRangeItem)itemToRemove);
  }

  internal async Task<ClosingValidationResult> UpdateSettings(
    bool isClosingWindow) {
    SettingsRanges.Clear();
    SettingsRanges.AddRange(
      from range in Ranges
      select new Settings.IntegerRange {
        Start = range.Start ?? 0,
        End = range.End ?? range.Start ?? 0
      });
    // Validating after saving, as the user can return to the page to fix the errors.
    var validation = await Validate(isClosingWindow);
    return !validation.Success
      ? validation
      : new ClosingValidationResult(true, true);
  }

  private async Task<ClosingValidationResult> Validate(bool isClosingWindow) {
    string errorMessage = GetErrorMessage();
    if (errorMessage == string.Empty) {
      return new ClosingValidationResult(true, true);
    }
    var errorReporter = new ErrorReporter(DialogService);
    bool canClosePage = await errorReporter.CanClosePageOnError(
      errorMessage, "MIDI for Macros", isClosingWindow, true);
    return new ClosingValidationResult(false, canClosePage);
  }
}