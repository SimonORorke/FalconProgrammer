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
      IsAddingAdditionItem) {
      Start = start,
      End = end
    });
  }

  protected abstract List<Settings.IntegerRange> GetRangesFromSettings();

  private static List<CcNoRangeViewModel> GetSortedRanges(
    IEnumerable<CcNoRangeViewModel> ranges) {
    var result = new List<CcNoRangeViewModel>(ranges);
    result.Sort((range1, range2) => {
      // Compare Starts first.
      int startComparison = range1.Start.CompareTo(range2.Start);
      return startComparison != 0 
        ? startComparison
        // Starts are same. (This is not allowed, but will be validated afterwards.)
        // So compare Ends. 
        : range1.End.CompareTo(range2.End);
    });
    return result;
  }

  internal void Populate(Settings settings) {
    IsPopulating = true;
    Settings = settings;
    Clear();
    foreach (var settingsRange in GetRangesFromSettings()) {
      AddItem(settingsRange.Start, settingsRange.End);
    }
    IsPopulating = false;
  }

  internal async Task<InteractiveValidationResult> UpdateSettingsAsync(
    bool isClosingWindow) {
    var sortedRanges = GetSortedRanges(
      from range in this
      where !range.IsAdditionItem
      select range);
    var validation = await ValidateAsync(sortedRanges, isClosingWindow); 
    if (!validation.Success) {
      return validation;
    }
    var settingsRanges = GetRangesFromSettings();
    settingsRanges.Clear();
    settingsRanges.AddRange(
      from range in sortedRanges
      select new Settings.IntegerRange {
        Start = range.Start,
        End = range.End
      });
    return new InteractiveValidationResult(true, true);
  }

  protected override void RemoveItem(ObservableObject itemToRemove) {
    RemoveItemTyped((CcNoRangeViewModel)itemToRemove);
  }

  private async Task<InteractiveValidationResult> ValidateAsync(
    IReadOnlyList<CcNoRangeViewModel> sortedRanges, bool isClosingWindow) {
    if (sortedRanges.Count == 0) {
      return new InteractiveValidationResult(true, true);
    }
    bool isValid = true;
    var previousRange = sortedRanges[0];
    for (int i = 1; i < sortedRanges.Count; i++) {
      var range = sortedRanges[i];
      if (range.Start == previousRange.Start || range.End <= previousRange.End) {
        isValid = false;
        break;
      }
    }
    if (isValid) {
      return new InteractiveValidationResult(true, true);
    }
    var errorReporter = new ErrorReporter(dialogService);
    bool canCloseWindow = await errorReporter.CanCloseWindowOnErrorAsync(
      $"{rangeType} CC No settings cannot be saved because their ranges overlap.", 
      isClosingWindow);
    return new InteractiveValidationResult(false, canCloseWindow);
  }
}