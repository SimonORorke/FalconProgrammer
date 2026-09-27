namespace FalconProgrammer.Model;

/// <summary>
///   A utility for accessing and updating the file system.
/// </summary>
public class FileSystemService : IFileSystemService {
  private FileSystemService() { }
  public static IFileSystemService Default => field ??= new FileSystemService();
  public IFileService File { get; } = new FileService();
  public IFolderService Folder { get; } = new FolderService();
}