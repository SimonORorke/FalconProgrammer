﻿using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class CcNoRangeCollection : DataGridItemCollectionBase<CcNoRangeItem> {
  public CcNoRangeCollection(string rangeType,
    IDialogService dialogService,
    IDispatcherService dispatcherService) : base(dispatcherService) {
    RangeType = rangeType;
    DialogService = dialogService;
  }

  private IDialogService DialogService { get; }
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

  protected override void CutItem(DataGridItemBase itemToCut) {
    CutItemTyped((CcNoRangeItem)itemToCut);
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
    var ranges = (from range in this
      where !range.IsAdditionItem
      select range).ToList();
    var validation = await Validate(ranges,
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

  private async Task<ClosingValidationResult> Validate(
    IReadOnlyCollection<CcNoRangeItem> ranges, bool isClosingWindow) {
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
    var errorReporter = new ErrorReporter(DialogService);
    bool canClosePage = await errorReporter.CanClosePageOnError(
      $"MIDI for Macros settings cannot be saved because {RangeType} CC No ranges " +
      "include overlapping ranges.", "MIDI for Macros",
      isClosingWindow);
    return new ClosingValidationResult(false, canClosePage);
  }
}