using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public partial class BackgroundItem : SoundBankItem {
  /// <summary>
  ///   Generates <see cref="Path" /> property.
  /// </summary>
  [ObservableProperty] private string _path = string.Empty;

  public BackgroundItem(Settings settings, IFileSystemService fileSystemService,
    bool isAdditionItem, Func<BackgroundItem, Task> browseForItemPath) :
    base(settings, fileSystemService, isAdditionItem) {
    BrowseForItemPath = browseForItemPath;
  }

  private Func<BackgroundItem, Task> BrowseForItemPath { get; }

  /// <summary>
  ///   Generates <see cref="BrowseCommand" />.
  /// </summary>
  [RelayCommand]
  private async Task Browse() {
    await BrowseForItemPath(this);
  }
}