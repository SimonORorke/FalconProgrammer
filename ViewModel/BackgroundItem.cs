using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.Model;
using FalconProgrammer.Model.Options;

namespace FalconProgrammer.ViewModel;

public partial class BackgroundItem : SoundBankItem {
  /// <summary>
  ///   Generates <see cref="Path" /> property.
  /// </summary>
  [ObservableProperty] private string _path = string.Empty;

  public BackgroundItem(Settings settings, IFileSystemService fileSystemService,
    bool isAdditionItem, Func<BackgroundItem, Task> browseForPath) :
    base(settings, fileSystemService, isAdditionItem) {
    BrowseForPath = browseForPath;
  }

  private Func<BackgroundItem, Task> BrowseForPath { get; }

  /// <summary>
  ///   Generates <see cref="BrowseCommand" />.
  /// </summary>
  [RelayCommand]
  private async Task Browse() {
    await BrowseForPath(this);
  }
}