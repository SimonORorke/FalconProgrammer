using System.Collections.Immutable;

namespace FalconProgrammer.Model;

/// <summary>
///   A utility for accessing and updating the file system.
/// </summary>
public class FileSystemService : IFileSystemService {
  private static IFileSystemService? _default;
  private FileSystemService() { }
  public static IFileSystemService Default => _default ??= new FileSystemService();

  public void CreateFolder(string path) {
    Directory.CreateDirectory(path);
  }

  public bool FileExists(string path) {
    return File.Exists(path);
  }

  public bool FolderExists(string path) {
    return Directory.Exists(path);
  }

  public ImmutableList<string> GetSubfolderNames(string path) {
    if (FolderExists(path)) {
      return Directory.GetDirectories(path).ToImmutableList();
    }
    throw new DirectoryNotFoundException($"Cannot find folder '{path}'.");
  }
}