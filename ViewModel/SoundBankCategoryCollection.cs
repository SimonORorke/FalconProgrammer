using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class SoundBankCategoryCollection(
  Settings settings,
  IFileSystemService fileSystemService)
  : ObservableCollection<SoundBankCategory> {
  private IFileSystemService FileSystemService { get; } = fileSystemService;
  private bool ForceAppendAdditionItem { get; set; }
  private bool IsPopulating { get; set; }

  /// <summary>
  ///   Gets whether the collection has changed since <see cref="Populate" /> was run.
  /// </summary>
  internal bool HasBeenChanged { get; private set; }

  private Settings Settings { get; } = settings;
  private ImmutableList<string> SoundBanks { get; set; } = [];
  private Action<Action> InvokeAsync { get; set; } = null!;

  private void AddItem(string soundBank, string category) {
    // string actionButtonText = SoundBankCategory.RemoveButtonText) {
    Add(new SoundBankCategory(
      Settings, FileSystemService, AppendAdditionItem, RemoveItem) {
      SoundBanks = SoundBanks,
      SoundBank = soundBank,
      Category = category,
      ActionButtonText = IsPopulating
        ? SoundBankCategory.RemoveButtonText
        : SoundBankCategory.AddButtonText
    });
  }

  /// <summary>
  ///   Appends an addition item.
  /// </summary>
  private void AppendAdditionItem() {
    if (IsPopulating & !ForceAppendAdditionItem) {
      return;
    }
    // AddItem(string.Empty, string.Empty);
    AddItem(SoundBankCategory.AdditionCaption, SoundBankCategory.AdditionCaption);
  }

  protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
    base.OnCollectionChanged(e);
    if (IsPopulating) {
      return;
    }
    HasBeenChanged = true;
  }

  protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
    base.OnPropertyChanged(e);
    if (IsPopulating) {
      return;
    }
    if (e.PropertyName is nameof(SoundBankCategory.SoundBank)
        or nameof(SoundBankCategory.Category)) {
      HasBeenChanged = true;
    }
  }

  internal void Populate(IEnumerable<string> soundBanks, Action<Action> invokeAsync) {
    // internal void Populate(List<string> soundBanks, Action<Action> invokeAsync) {
    InvokeAsync = invokeAsync;
    IsPopulating = true;
    // // Allow a SoundBankCategory to be removed.
    // // soundBanks.Insert(0, string.Empty);
    // soundBanks.Insert(0, SoundBankCategory.RemovalCaption);
    SoundBanks = soundBanks.ToImmutableList();
    Clear();
    foreach (var category in Settings.MustUseGuiScriptProcessorCategories) {
      string categoryToDisplay = string.IsNullOrWhiteSpace(category.Category)
        ? SoundBankCategory.AllCaption
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
    InvokeAsync(() => Remove(itemToRemove));
  }
}