using System.Collections.Immutable;
using FalconProgrammer.Model;
using FalconProgrammer.Model.Options;

namespace FalconProgrammer.ViewModel;

public abstract class SoundBankCollection :
  ProgramHierarchyCollectionBase<SoundBankItem> {
  protected SoundBankCollection(
    IFileSystemService fileSystemService,
    IDispatcherService dispatcherService) : base(
    fileSystemService, dispatcherService) { }

  protected abstract List<string> SettingsSoundBanks { get; }

  protected override void AppendAdditionItem() {
    AddItem();
  }

  private void AddItem(string soundBank = "") {
    AddItem(new SoundBankItem(Settings, FileSystemService, IsAddingAdditionItem) {
      SoundBanks = SoundBanks,
      SoundBank = soundBank
    });
  }

  protected override void CutItem(DataGridItemBase itemToCut) {
    CutItemTyped((SoundBankItem)itemToCut);
  }

  protected override void PasteBeforeItem(DataGridItemBase itemBeforeWhichToPaste) {
    PasteBeforeItemTyped((SoundBankItem)itemBeforeWhichToPaste);
  }

  internal override void Populate(Settings settings, IEnumerable<string> soundBanks) {
    IsPopulating = true;
    Settings = settings;
    SoundBanks = soundBanks.ToImmutableList();
    Clear();
    foreach (string soundBank in SettingsSoundBanks) {
      AddItem(soundBank);
    }
    IsPopulating = false;
  }

  protected override void RemoveItem(DataGridItemBase itemToRemove) {
    RemoveItemTyped((SoundBankItem)itemToRemove);
  }

  internal override void UpdateSettings() {
    SettingsSoundBanks.Clear();
    foreach (var soundBankItem in this) {
      if (!soundBankItem.IsAdditionItem && soundBankItem.SoundBank != string.Empty) {
        SettingsSoundBanks.Add(soundBankItem.SoundBank);
      }
    }
    Settings.Write();
  }
}