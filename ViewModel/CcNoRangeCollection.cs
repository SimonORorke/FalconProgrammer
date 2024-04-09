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

  protected abstract IEnumerable<Settings.IntegerRange> GetRangesFromSettings();
  
  internal void Populate() {
    IsPopulating = true;
    Clear();
    foreach (var range in GetRangesFromSettings()) {
      AddItem(range.Start, range.End);
    }
    IsPopulating = false;
  }

  protected override void RemoveItem(ObservableObject itemToRemove) {
    RemoveItemTyped((CcNoRangeViewModel)itemToRemove);
  }
}