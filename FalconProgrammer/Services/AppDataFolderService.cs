using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Services;

public class AppDataFolderService : IAppDataFolderService {
  private static IAppDataFolderService? _default;
  private AppDataFolderService() { }
  public static IAppDataFolderService Default => _default ??= new AppDataFolderService();
  public string AppDataFolderPathMaui { get; set; } = FileSystem.AppDataDirectory;
}