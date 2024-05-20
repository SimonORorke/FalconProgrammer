using System.Collections.Immutable;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public class BackgroundCollection : ProgramHierarchyCollection<BackgroundItem> {
  public BackgroundCollection(IDialogService dialogService,
    IFileSystemService fileSystemService,
    IDispatcherService dispatcherService) : base(
    fileSystemService, dispatcherService) {
    DialogService = dialogService;
  }

  private IDialogService DialogService { get; }

  protected override void AppendAdditionItem() {
    AddItem();
  }

  private void AddItem(string soundBank = "", string path = "") {
    AddItem(new BackgroundItem(Settings, FileSystemService, IsAddingAdditionItem,
      BrowseForPath) {
      SoundBanks = SoundBanks,
      SoundBank = soundBank,
      Path = path
    });
  }

  private async Task BrowseForPath(BackgroundItem item) {
    string? path = await DialogService.OpenFile(
      "Select a background image (dimensions: 720 x 480)",
      "Images", "png");
    if (path != null) {
      item.Path = path;
    }
  }

  protected override void CutItem(DataGridItemBase itemToCut) {
    CutItemTyped((BackgroundItem)itemToCut);
  }

  protected override void PasteBeforeItem(DataGridItemBase itemBeforeWhichToPaste) {
    PasteBeforeItemTyped((BackgroundItem)itemBeforeWhichToPaste);
  }

  internal override void Populate(Settings settings, IEnumerable<string> soundBanks) {
    IsPopulating = true;
    Settings = settings;
    SoundBanks = soundBanks.ToImmutableList();
    Clear();
    foreach (var background in Settings.Backgrounds) {
      AddItem(background.SoundBank, background.Path);
    }
    IsPopulating = false;
  }

  protected override void RemoveItem(DataGridItemBase itemToRemove) {
    RemoveItemTyped((BackgroundItem)itemToRemove);
  }

  internal override void UpdateSettings() {
    Settings.Backgrounds.Clear();
    foreach (var backgroundItem in this) {
      if (!backgroundItem.IsAdditionItem && backgroundItem.SoundBank != string.Empty) {
        Settings.Backgrounds.Add(new Settings.Background {
          SoundBank = backgroundItem.SoundBank,
          Path = backgroundItem.Path
        });
      }
    }
    Settings.Write();
  }
}