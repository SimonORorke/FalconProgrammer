namespace FalconProgrammer.Model;

/// <summary>
///   A utility for accessing and updating the file system.
/// </summary>
public interface IFileSystemService {
  IFileService File { get; }
  IFolderService Folder { get; }
}