using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class CcNoRangeCollection(
  string rangeType,
  IDialogService dialogService,
  IDispatcherService dispatcherService)
  : DataGridItemCollection<CcNoRangeViewModel>(dispatcherService) {
  protected override void AppendAdditionItem() {
    AddItem();
  }

  private void AddItem(int start = 0, int end = 127) {
    Add(new CcNoRangeViewModel(
      AppendAdditionItem, OnItemChanged, RemoveItem,
      IsAddingAdditionItem, CutItem, PasteBeforeItem) {
      Start = start,
      End = end
    });
  }

  protected abstract List<Settings.IntegerRange> GetRangesFromSettings();

  internal void Populate(Settings settings) {
    IsPopulating = true;
    Settings = settings;
    Clear();
    foreach (var settingsRange in GetRangesFromSettings()) {
      AddItem(settingsRange.Start, settingsRange.End);
    }
    IsPopulating = false;
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
    var settingsRanges = GetRangesFromSettings();
    settingsRanges.Clear();
    settingsRanges.AddRange(
      from range in ranges
      select new Settings.IntegerRange {
        Start = range.Start,
        End = range.End
      });
    return new ClosingValidationResult(true, true);
  }

  protected override void RemoveItem(DataGridItem itemToRemove) {
    RemoveItemTyped((CcNoRangeViewModel)itemToRemove);
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