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
    var item = new BackgroundItem(Settings, FileSystemService, IsAddingAdditionItem) {
      SoundBanks = SoundBanks,
      SoundBank = soundBank,
      Path = path
    };
    item.BrowseForPath -= ItemOnBrowseForPath;
    item.BrowseForPath += ItemOnBrowseForPath;
    AddItem(item);
  }

  private async Task BrowseForPath(BackgroundItem item) {
    string? path = await DialogService.BrowseForFile(
      "Select a background image (dimensions: 720 x 480)",
      "Images", "png");
    if (path != null) {
      item.Path = path;
    }
  }

  protected override void CutItem(DataGridItem itemToCut) {
    CutItemTyped((BackgroundItem)itemToCut);
  }

  private void ItemOnBrowseForPath(object? sender, BackgroundItem e) {
    DispatcherService.Dispatch(() => Task.Run(() => BrowseForPath(e)));
  }

  protected override void PasteBeforeItem(DataGridItem itemBeforeWhichToPaste) {
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

  protected override void RemoveItem(DataGridItem itemToRemove) {
    RemoveItemTyped((BackgroundItem)itemToRemove);
  }

  internal override void UpdateSettings() {
    Settings.Backgrounds.Clear();
    foreach (var backgroundItem in this) {
      if (!backgroundItem.IsAdditionItem) {
        Settings.Backgrounds.Add(new Settings.Background {
          SoundBank = backgroundItem.SoundBank,
          Path = backgroundItem.Path
        });
      }
    }
    Settings.Write();
  }
}