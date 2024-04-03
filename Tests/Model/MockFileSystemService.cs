using System.Collections.Immutable;
using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockFileSystemService : IFileSystemService {
  internal bool ExpectedFileExists { get; set; } = true;
  internal bool ExpectedFolderExists { get; set; } = true;
  internal List<string> ExistingFolderPaths { get; } = [];

  internal Dictionary<string, IEnumerable<string>> ExpectedPathsOfFilesInFolder { get; } =
    [];

  internal Dictionary<string, IEnumerable<string>> ExpectedSubfolderNames { get; } = [];
  public void CreateFolder(string path) { }

  public bool FileExists(string path) {
    return ExpectedFileExists;
  }

  public bool FolderExists(string path) {
    if (ExistingFolderPaths.Count == 0) {
      return ExpectedFolderExists;
    }
    if (ExistingFolderPaths.Contains(path)) {
      return true;
    }
    return (
      from folderPath in ExistingFolderPaths
      where Directory.GetParent(folderPath).FullName == path
      select folderPath).Any();
  }

  public IEnumerable<string> GetPathsOfFilesInFolder(string path, string searchPattern) {
    return ExpectedPathsOfFilesInFolder[path];
  }

  public ImmutableList<string> GetSubfolderNames(string path) {
    return ExpectedSubfolderNames[path].ToImmutableList();
  }
}