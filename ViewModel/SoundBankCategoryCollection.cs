using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class SoundBankCategoryCollection(
  IFileSystemService fileSystemService)
  : ObservableCollection<SoundBankCategory> {
  private IDispatcherService DispatcherService { get; set; } = null!;
  private IFileSystemService FileSystemService { get; } = fileSystemService;
  private bool ForceAppendAdditionItem { get; set; }
  private bool IsPopulating { get; set; }

  /// <summary>
  ///   Gets whether the collection has changed since <see cref="Populate" /> was run.
  /// </summary>
  internal bool HasBeenChanged { get; private set; }

  private Settings Settings { get; set; } = null!;
  private ImmutableList<string> SoundBanks { get; set; } = [];

  private void AddItem(string soundBank = "", string category = "") {
    Add(new SoundBankCategory(
      Settings, FileSystemService, AppendAdditionItem, OnItemChanged, RemoveItem) {
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

  internal void Populate(Settings settings, IEnumerable<string> soundBanks,
    IDispatcherService dispatcherService) {
    IsPopulating = true;
    Settings = settings;
    DispatcherService = dispatcherService;
    SoundBanks = soundBanks.ToImmutableList();
    Clear();
    foreach (var category in Settings.MustUseGuiScriptProcessorCategories) {
      string categoryToDisplay = string.IsNullOrWhiteSpace(category.Category)
        ? SoundBankCategory.AllCategoriesCaption
        : category.Category;
      AddItem(category.SoundBank, categoryToDisplay);
    }
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