using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public partial class BackgroundItem : SoundBankItem {
  [ObservableProperty] private string _path = string.Empty; // Generates Path property

  public BackgroundItem(Settings settings, IFileSystemService fileSystemService,
    bool isAdditionItem, Func<BackgroundItem, Task> browseForItemPath) :
    base(settings, fileSystemService, isAdditionItem) {
    BrowseForItemPath = browseForItemPath;
  }

  private Func<BackgroundItem, Task> BrowseForItemPath { get; }

  [RelayCommand] // Generates BrowseCommand
  private async Task Browse() {
    await BrowseForItemPath(this);
  }
}