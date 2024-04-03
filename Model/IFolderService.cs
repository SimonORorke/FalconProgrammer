using System.Collections.Immutable;

namespace FalconProgrammer.Model;

/// <summary>
///   A utility for accessing folders.
/// </summary>
public interface IFolderService {
  void Create(string path);
  bool Exists(string path);
  IEnumerable<string> GetFilePaths(string path, string searchPattern);
  ImmutableList<string> GetSubfolderNames(string path);
}