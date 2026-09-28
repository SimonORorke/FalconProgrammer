using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;
using FalconProgrammer.Model.ReleaseInfo;

namespace FalconProgrammer.ViewModel;

/// <summary>
///   Services defined in the model that are used in the view model.
/// </summary>
public class ModelServices {
  public IFileSystemService FileSystemService {
    [ExcludeFromCodeCoverage] get => field ??= Model.FileSystemService.Default;
    set;
    // For tests
  }

  public SettingsFolderLocationReader SettingsFolderLocationReader {
    [ExcludeFromCodeCoverage] get => field ??= new SettingsFolderLocationReader();
    set;
    // For tests
  }

  public SettingsReader SettingsReader {
    [ExcludeFromCodeCoverage] get => field ??= new SettingsReader();
    set;
    // For tests
  }

  public IReleaseInfo ReleaseInfo {
    [ExcludeFromCodeCoverage] get => field ??= new ReleaseInfo();
    set;
    // For tests
  }

  public IVersionChecker VersionChecker {
    [ExcludeFromCodeCoverage] get => field ??= new VersionChecker(ReleaseInfo);
    set;
    // For tests
  }
}