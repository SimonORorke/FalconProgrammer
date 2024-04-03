namespace FalconProgrammer.Model;

/// <summary>
///   A utility for accessing and updating the file system.
/// </summary>
public class FileSystemService : IFileSystemService {
  private static IFileSystemService? _default;
  private FileSystemService() { }
  public static IFileSystemService Default => _default ??= new FileSystemService();
  public IFileService File { get; } = new FileService();
  public IFolderService Folder { get; } = new FolderService();
}