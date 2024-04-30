using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockFolderService : IFolderService {
  internal bool CanCreate { get; set; } = true;
  internal bool ExpectedExists { get; set; } = true;
  internal List<string> ExistingPaths { get; } = [];

  internal Dictionary<string, IEnumerable<string>> ExpectedFilePaths { get; } =
    [];

  internal Dictionary<string, IEnumerable<string>> ExpectedSubfolderNames { get; } = [];

  [ExcludeFromCodeCoverage]
  public void Create(string path) {
    if (!CanCreate) {
      throw new DirectoryNotFoundException();
    }
  }

  public bool Exists(string path) {
    if (ExistingPaths.Count == 0) {
      return ExpectedExists;
    }
    if (ExistingPaths.Contains(path)) {
      return true;
    }
    return (
      from folderPath in ExistingPaths
      where Directory.GetParent(folderPath)?.FullName == path
      select folderPath).Any();
  }

  public IEnumerable<string> GetFilePaths(string path, string searchPattern) {
    return ExpectedFilePaths.TryGetValue(path, out var value)
      ? value
      : [];
  }

  public ImmutableList<string> GetSubfolderNames(string path) {
    return ExpectedSubfolderNames[path].ToImmutableList();
  }
}