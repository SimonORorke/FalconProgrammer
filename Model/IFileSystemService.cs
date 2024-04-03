using System.Collections.Immutable;

namespace FalconProgrammer.Model;

/// <summary>
///   A utility for accessing and updating the file system.
/// </summary>
public interface IFileSystemService {
  void CreateFolder(string path);
  bool FileExists(string path);
  bool FolderExists(string path);
  IEnumerable<string> GetPathsOfFilesInFolder(string path, string searchPattern);
  ImmutableList<string> GetSubfolderNames(string path);
}