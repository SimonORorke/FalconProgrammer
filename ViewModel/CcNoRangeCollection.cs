using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class CcNoRangeCollection(
  string rangeType,
  IDialogService dialogService,
  IDispatcherService dispatcherService)
  : DataGridItemCollection<CcNoRangeViewModel>(dispatcherService) {
  private List<Settings.IntegerRange> SettingsRanges { get; set; } = null!;

  protected override void AppendAdditionItem() {
    AddItem();
  }

  private void AddItem(int? start = null, int? end = null) {
    Add(new CcNoRangeViewModel(
      AppendAdditionItem, OnItemChanged, RemoveItem,
      IsAddingAdditionItem, CutItem, PasteBeforeItem) {
      Start = start,
      End = end
    });
  }

  protected override void CutItem(DataGridItem itemToCut) {
    CutItemTyped((CcNoRangeViewModel)itemToCut);
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

  protected override void PasteBeforeItem(DataGridItem itemBeforeWhichToPaste) {
    PasteBeforeItemTyped((CcNoRangeViewModel)itemBeforeWhichToPaste);
  }

  protected override void RemoveItem(DataGridItem itemToRemove) {
    RemoveItemTyped((CcNoRangeViewModel)itemToRemove);
  }

  internal async Task<ClosingValidationResult> UpdateSettingsAsync(
    bool isClosingWindow) {
    var ranges = (from range in this
      where !range.IsAdditionItem
      select range).ToList();
    var validation = await ValidateAsync(ranges,
      isClosingWindow);
    if (!validation.Success) {
      return validation;
    }
    SettingsRanges.Clear();
    SettingsRanges.AddRange(
      from range in ranges
      select new Settings.IntegerRange {
        Start = range.Start ?? 0,
        End = range.End ?? range.Start ?? 0
      });
    return new ClosingValidationResult(true, true);
  }

  private async Task<ClosingValidationResult> ValidateAsync(
    IReadOnlyCollection<CcNoRangeViewModel> ranges, bool isClosingWindow) {
    if (ranges.Count == 0) {
      return new ClosingValidationResult(true, true);
    }
    bool isValid = true;
    foreach (var range in ranges) {
      var otherRanges =
        from otherRange in ranges
        where otherRange != range
        select otherRange;
      if (otherRanges.Select(otherRange =>
            otherRange.Start <= range.End && range.Start <= otherRange.End)
          .Any(overlap => overlap)) {
        isValid = false;
      }
    }
    if (isValid) {
      return new ClosingValidationResult(true, true);
    }
    var errorReporter = new ErrorReporter(dialogService);
    bool canClosePage = await errorReporter.CanClosePageOnErrorAsync(
      $"MIDI for Macros settings cannot be saved because {rangeType} CC No ranges " +
      "include overlapping ranges.",
      isClosingWindow);
    return new ClosingValidationResult(false, canClosePage);
  }
}