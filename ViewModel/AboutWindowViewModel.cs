using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FalconProgrammer.ViewModel;

public partial class AboutWindowViewModel : ObservableObject {
  private IApplicationInfo? _applicationInfo;

  internal IApplicationInfo ApplicationInfo {
    get => _applicationInfo ??= new ApplicationInfo();
    // The setter is for tests.
    set => _applicationInfo = value;
  }

  public string Copyright => ApplicationInfo.Copyright;

  [ExcludeFromCodeCoverage]
  public static string DownloadUrl => "https://github.com/SimonORorke/FalconProgrammer";

  public string Product => ApplicationInfo.Product;
  public string Title => $"About {ApplicationInfo.Product}";
  public string Version => ApplicationInfo.Version;

  public event EventHandler? MustClose;

  /// <summary>
  ///   Generates <see cref="LicenceCommand" />.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [RelayCommand]
  private static void Licence() {
    string path = Path.Combine(
      Directory.GetParent(Environment.ProcessPath!)!.FullName, "LICENCE.txt");
    if (File.Exists(path)) {
      Process.Start(new ProcessStartInfo(path) {
        UseShellExecute = true
      });
    }
  }

  /// <summary>
  ///   Generates <see cref="OkCommand" />.
  /// </summary>
  [RelayCommand]
  private void Ok() {
    OnMustClose();
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

  private void OnMustClose() {
    MustClose?.Invoke(this, EventArgs.Empty);
  }
}