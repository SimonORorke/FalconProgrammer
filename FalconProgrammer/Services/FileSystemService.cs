using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Services;

public class FileSystemService : IFileSystemService {
  private static IFileSystemService? _default;
  public static IFileSystemService Default => _default ??= new FileSystemService();

  public string AppDataFolderPathMaui { get; set; } = FileSystem.AppDataDirectory;

  public bool FileExists(string path) {
    return File.Exists(path);
  }

  public bool FolderExists(string path) {
    return Directory.Exists(path);
  }
}