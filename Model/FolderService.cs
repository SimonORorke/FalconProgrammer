using System.Collections.Immutable;

namespace FalconProgrammer.Model;

public class FolderService : IFolderService {
  public void Create(string path) {
    Directory.CreateDirectory(path);
  }

  public bool Exists(string path) {
    return Directory.Exists(path);
  }

  public IEnumerable<string> GetFilePaths(string path, string searchPattern) {
    if (Exists(path)) {
      return Directory.GetFiles(path, searchPattern);
    }
    throw new DirectoryNotFoundException($"Cannot find folder '{path}'.");
  }

  public ImmutableList<string> GetSubfolderNames(string path) {
    if (Exists(path)) {
      var subfolderNames = (
        from subfolderPath in Directory.GetDirectories(path)
        select Path.GetFileName(subfolderPath)).ToImmutableList();
      return subfolderNames;
    }
    throw new DirectoryNotFoundException($"Cannot find folder '{path}'.");
  }
}