using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public abstract class CcNoRangeCollection(
  Settings settings,
  IDispatcherService dispatcherService)
  : DataGridItemCollection<CcNoRangeViewModel>(settings, dispatcherService) {
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
  
  internal void Populate() {
    IsPopulating = true;
    Clear();
    foreach (var settingsRange in GetRangesFromSettings()) {
      AddItem(settingsRange.Start, settingsRange.End);
    }
    IsPopulating = false;
  }

  internal void UpdateSettings() {
    var settingsRanges = GetRangesFromSettings();
    settingsRanges.Clear();
    settingsRanges.AddRange(
      from range in this
      where !range.IsAdditionItem
      select new Settings.IntegerRange {
        Start = range.Start, 
        End = range.End
      });
  }

  protected override void RemoveItem(ObservableObject itemToRemove) {
    RemoveItemTyped((CcNoRangeViewModel)itemToRemove);
  }
}