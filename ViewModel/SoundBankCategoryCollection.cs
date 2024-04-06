using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace FalconProgrammer.ViewModel;

public class SoundBankCategoryCollection : ObservableCollection<SoundBankCategory> {
  private IDispatcherService DispatcherService { get; set; } = null!;
  private bool ForceAppendAdditionItem { get; set; }
  private bool IsPopulating { get; set; }

  /// <summary>
  ///   Gets whether the collection has changed since <see cref="Populate" /> was run.
  /// </summary>
  internal bool HasBeenChanged { get; private set; }

  private ImmutableList<string> SoundBanks { get; set; } = [];

  private void AddItem(string soundBank = "", string category = "") {
    Add(new SoundBankCategory(AppendAdditionItem, OnItemChanged, RemoveItem) {
      SoundBanks = SoundBanks,
      SoundBank = soundBank,
      Category = category,
      CanRemove = IsPopulating && !ForceAppendAdditionItem
    });
  }

  /// <summary>
  ///   Appends an addition item.
  /// </summary>
  private void AppendAdditionItem() {
    if (IsPopulating & !ForceAppendAdditionItem) {
      return;
    }
    AddItem();
  }

  protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
    base.OnCollectionChanged(e);
    if (IsPopulating) {
      return;
    }
    HasBeenChanged = true;
  }

  private void OnItemChanged() {
    HasBeenChanged = true;
  }

  internal void Populate(
    IEnumerable<string> soundBanks, IDispatcherService dispatcherService) {
    IsPopulating = true;
    DispatcherService = dispatcherService;
    SoundBanks = soundBanks.ToImmutableList();
    Clear();
    foreach (string soundBank in SoundBanks) {
      AddItem(soundBank, SoundBankCategory.AllCategoriesCaption);
    }
    // AddItem("Pulsar", "Bass");
    // AddItem("Pulsar", "Lead");
    // AddItem("Titanium", SoundBankCategory.AllCategoriesCaption);
    ForceAppendAdditionItem = true;
    AppendAdditionItem();
    ForceAppendAdditionItem = false;
    IsPopulating = false;
    HasBeenChanged = false;
  }

  private void RemoveItem(SoundBankCategory itemToRemove) {
    DispatcherService.Dispatch(() => Remove(itemToRemove));
  }
}