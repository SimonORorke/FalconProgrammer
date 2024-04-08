using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class SoundBankCategoryCollection(
  Settings settings,
  IFileSystemService fileSystemService,
  IDispatcherService dispatcherService)
  : DataGridItemCollection<SoundBankCategory>(settings, dispatcherService) {
  private IFileSystemService FileSystemService { get; } = fileSystemService;

  private ImmutableList<string> SoundBanks { get; set; } = [];

  protected override void AddAdditionItem() {
    AddItem();
  }

  private void AddItem(string soundBank = "", string category = "") {
    Add(new SoundBankCategory(
      Settings, FileSystemService, AppendAdditionItem, OnItemChanged, RemoveItem, 
      IsAddingAdditionItem) {
      SoundBanks = SoundBanks,
      SoundBank = soundBank,
      Category = category
    });
  }

  internal void Populate(IEnumerable<string> soundBanks) {
    IsPopulating = true;
    SoundBanks = soundBanks.ToImmutableList();
    Clear();
    foreach (var category in Settings.MustUseGuiScriptProcessorCategories) {
      string categoryToDisplay = string.IsNullOrWhiteSpace(category.Category)
        ? SoundBankCategory.AllCategoriesCaption
        : category.Category;
      AddItem(category.SoundBank, categoryToDisplay);
    }
    IsPopulating = false;
  }

  private void RemoveItem(ObservableObject itemToRemove) {
    RemoveItemTyped((SoundBankCategory)itemToRemove);
  }
}