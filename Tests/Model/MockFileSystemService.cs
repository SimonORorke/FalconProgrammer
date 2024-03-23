using System.Collections.Immutable;
using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockFileSystemService : IFileSystemService {
  internal bool ExpectedFileExists { get; set; } = true;
  internal bool ExpectedFolderExists { get; set; } = true;
  internal Dictionary<string, IEnumerable<string>> ExpectedSubfolderNames { get; } = [];
  
  public void CreateFolder(string path) {
  }

  public bool FileExists(string path) {
    return ExpectedFileExists;
  }

  public bool FolderExists(string path) {
    return ExpectedFolderExists;
  }

  public ImmutableList<string> GetSubfolderNames(string path) {
    return ExpectedSubfolderNames[path].ToImmutableList();
  }
}