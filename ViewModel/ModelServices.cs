using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

/// <summary>
///   Services defined in the model that are used in the view model.
/// </summary>
public class ModelServices {
  private IFileSystemService? _fileSystemService;
  private SettingsFolderLocationReader? _settingsFolderLocationReader;
  private SettingsReader? _settingsReader;

  public IFileSystemService FileSystemService {
    [ExcludeFromCodeCoverage]
    get => _fileSystemService ??= Model.FileSystemService.Default;
    set => _fileSystemService = value; // For tests
  }

  public SettingsFolderLocationReader SettingsFolderLocationReader {
    [ExcludeFromCodeCoverage]
    get => _settingsFolderLocationReader ??= new SettingsFolderLocationReader();
    set => _settingsFolderLocationReader = value; // For tests
  }

  public SettingsReader SettingsReader {
    [ExcludeFromCodeCoverage] get => _settingsReader ??= new SettingsReader();
    set => _settingsReader = value; // For tests
  }
}