using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public partial class AboutWindowViewModel {
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

  public string Product => ApplicationInfo.Product;
  public string Title => $"About {ApplicationInfo.Product}";

  [ExcludeFromCodeCoverage]
  public static string Url => "https://github.com/SimonORorke/FalconProgrammer";
  
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
      new MessageWindowViewModel(licenceText, "Licence") {
        ApplicationInfo = ApplicationInfo
      });
  }

  /// <summary>
  ///   Generates <see cref="OpenUrlCommand" />.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [RelayCommand]
  private void OpenUrl() {
    Process.Start(new ProcessStartInfo(Url) {
      UseShellExecute = true
    });
  }
}