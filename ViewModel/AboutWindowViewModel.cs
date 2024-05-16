using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public partial class AboutWindowViewModel : ObservableObject {
  private IApplicationInfo? _applicationInfo;

  public AboutWindowViewModel(IDialogService dialogService) {
    DialogService = dialogService;
  }

  internal IApplicationInfo ApplicationInfo {
    get => _applicationInfo ??= new ApplicationInfo();
    // The setter is for tests.
    set => _applicationInfo = value;
  }

  public string Copyright => ApplicationInfo.Copyright;
  private IDialogService DialogService { get; }

  [ExcludeFromCodeCoverage]
  public static string DownloadUrl => "https://github.com/SimonORorke/FalconProgrammer";

  public string Product => ApplicationInfo.Product;
  public string Title => $"About {ApplicationInfo.Product}";
  public string Version => ApplicationInfo.Version;

  /// <summary>
  ///   Generates <see cref="LicenceCommand" />.
  /// </summary>
  [RelayCommand]
  private async Task Licence() {
    var stream = Global.GetEmbeddedFileStream("LICENCE.txt");
    var reader = new StreamReader(stream);
    string licenceText = await reader.ReadToEndAsync();
    await DialogService.ShowMessageWindow(
      new MessageWindowViewModel(licenceText, $"{Product} - Licence"));
  }

  /// <summary>
  ///   Generates <see cref="OpenDownloadUrlCommand" />.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [RelayCommand]
  private void OpenDownloadUrl() {
    Process.Start(new ProcessStartInfo(DownloadUrl) {
      UseShellExecute = true
    });
  }
}