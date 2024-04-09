using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class SoundBankCategoryCollection(
  IFileSystemService fileSystemService,
  IDispatcherService dispatcherService)
  : DataGridItemCollection<SoundBankCategory>(dispatcherService) {
  private IFileSystemService FileSystemService { get; } = fileSystemService;

  private ImmutableList<string> SoundBanks { get; set; } = [];

  protected override void AppendAdditionItem() {
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

  internal void Populate(Settings settings, IEnumerable<string> soundBanks) {
    IsPopulating = true;
    Settings = settings;
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

  protected override void RemoveItem(ObservableObject itemToRemove) {
    RemoveItemTyped((SoundBankCategory)itemToRemove);
  }

  internal void UpdateSettings() {
    Settings.MustUseGuiScriptProcessorCategories.Clear();
    foreach (var soundBankCategory in this) {
      if (!soundBankCategory.IsAdditionItem) {
        Settings.MustUseGuiScriptProcessorCategories.Add(
          new Settings.ProgramCategory {
            SoundBank = soundBankCategory.SoundBank,
            Category = soundBankCategory.IsForAllCategories
              ? string.Empty
              : soundBankCategory.Category
          });
      }
    }
    Settings.Write();
  }
}