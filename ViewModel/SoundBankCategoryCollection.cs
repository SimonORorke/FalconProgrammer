using System.Collections.Immutable;
using FalconProgrammer.Model;
using FalconProgrammer.Model.Options;

namespace FalconProgrammer.ViewModel;

public class
  SoundBankCategoryCollection : ProgramHierarchyCollectionBase<SoundBankCategory> {
  public SoundBankCategoryCollection(IFileSystemService fileSystemService,
    IDispatcherService dispatcherService) : base(fileSystemService, dispatcherService) { }

  protected override void AppendAdditionItem() {
    AddItem();
  }

  private void AddItem(string soundBank = "", string category = "") {
    var item = new SoundBankCategory(
      Settings, FileSystemService, IsAddingAdditionItem, true) {
      SoundBanks = SoundBanks,
      SoundBank = soundBank,
      Category = category
    };
    AddItem(item);
    if (item.Categories.Count == 1) { // 'All' only
      item.Category = SoundBankCategory.SoundBankErrorMessage;
    }
  }

  protected override void CutItem(DataGridItemBase itemToCut) {
    CutItemTyped((SoundBankCategory)itemToCut);
  }

  protected override void PasteBeforeItem(DataGridItemBase itemBeforeWhichToPaste) {
    PasteBeforeItemTyped((SoundBankCategory)itemBeforeWhichToPaste);
  }

  internal override void Populate(Settings settings, IEnumerable<string> soundBanks) {
    IsPopulating = true;
    Settings = settings;
    SoundBanks = soundBanks.ToImmutableList();
    Clear();
    foreach (var category in Settings.MustUseGuiScriptProcessorCategories) {
      string categoryToDisplay = string.IsNullOrWhiteSpace(category.Category)
        ? SoundBankItem.AllCaption
        : category.Category;
      AddItem(category.SoundBank, categoryToDisplay);
    }
    IsPopulating = false;
  }

  protected override void RemoveItem(DataGridItemBase itemToRemove) {
    RemoveItemTyped((SoundBankCategory)itemToRemove);
  }

  internal override void UpdateSettings() {
    Settings.MustUseGuiScriptProcessorCategories.Clear();
    foreach (var soundBankCategory in this) {
      if (!soundBankCategory.IsAdditionItem) {
        Settings.MustUseGuiScriptProcessorCategories.Add(
          new SoundBankCategorySetting {
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