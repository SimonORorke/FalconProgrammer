using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public partial class BackgroundItem : SoundBankItem {
  [ObservableProperty] private string _path = string.Empty; // Generates Path property

  public BackgroundItem(Settings settings, IFileSystemService fileSystemService,
    bool isAdditionItem) : base(settings, fileSystemService, isAdditionItem) { }

  [RelayCommand] // Generates BrowseCommand
  private async Task  Browse() {
    await OnBrowseForPath();
  }

  private async Task  OnBrowseForPath() {
    await BrowseForPath?.AsyncInvoke(null, this);
  }

  internal event EventHandler<BackgroundItem>? BrowseForPath;
}