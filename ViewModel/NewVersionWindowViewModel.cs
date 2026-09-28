using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FalconProgrammer.Model;
using FalconProgrammer.Model.ReleaseInfo;

namespace FalconProgrammer.ViewModel;

public partial class NewVersionWindowViewModel : ObservableObject {
  private bool _autoCheckNewVersions;
  private bool _ignoreThisVersion;
  private string _ignoreVersion = string.Empty;

  public NewVersionWindowViewModel(
    string latestVersion,
    bool autoCheckNewVersions,
    string ignoreVersion,
    IApplicationInfo? applicationInfo = null) {
    LatestVersion = latestVersion;
    _autoCheckNewVersions = autoCheckNewVersions;
    _ignoreVersion = ignoreVersion ?? string.Empty;
    _ignoreThisVersion = !string.IsNullOrWhiteSpace(_ignoreVersion) &&
                         VersionChecker.CompareVersions(latestVersion, _ignoreVersion) <= 0;
    ApplicationInfo = applicationInfo ?? new ApplicationInfo();
  }

  internal IApplicationInfo ApplicationInfo { get; set; }

  public string LatestVersion { get; }

  public string Title => "New Version";

  public string Message =>
    $"Version {LatestVersion} of {ApplicationInfo.Product} is available.";

  public string OpenReleasesPageCaption => "_Open Releases Page";

  public string AutoCheckNewVersionsCaption => "_Automatically check for new versions";

  public string IgnoreThisVersionCaption => "_Ignore this version";

  public bool AutoCheckNewVersions {
    get => _autoCheckNewVersions;
    set => SetProperty(ref _autoCheckNewVersions, value);
  }

  public bool IgnoreThisVersion {
    get => _ignoreThisVersion;
    set {
      if (SetProperty(ref _ignoreThisVersion, value)) {
        IgnoreVersion = value ? LatestVersion : string.Empty;
      }
    }
  }

  public string IgnoreVersion {
    get => _ignoreVersion;
    set => SetProperty(ref _ignoreVersion, value);
  }

  [ExcludeFromCodeCoverage]
  public static string ReleasesUrl => "https://github.com/SimonORorke/FalconProgrammer/releases";

  [ExcludeFromCodeCoverage]
  [RelayCommand]
  private static void OpenReleasesPage() {
    Process.Start(new ProcessStartInfo(ReleasesUrl) {
      UseShellExecute = true
    });
  }
}
