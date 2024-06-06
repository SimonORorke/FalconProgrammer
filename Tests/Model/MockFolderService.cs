using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockFolderService : IFolderService {
  internal bool CanCreate { get; set; } = true;
  internal List<string> ExistingPaths { get; } = [];
  internal bool SimulatedExists { get; set; } = true;
  internal bool ThrowIfNoSimulatedSubfolders { get; set; }
  internal Dictionary<string, IEnumerable<string>> SimulatedFilePaths { get; } = [];
  internal Dictionary<string, IEnumerable<string>> SimulatedSubfolderNames { get; } = [];

  [ExcludeFromCodeCoverage]
  public void Create(string path) {
    if (!CanCreate) {
      throw new DirectoryNotFoundException();
    }
  }

  public bool Exists(string path) {
    if (ExistingPaths.Count == 0) {
      return SimulatedExists;
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
    if (SimulatedFilePaths.TryGetValue(
          path, out var simulatedFilePaths)) {
      return simulatedFilePaths;
    }
    if (!ThrowIfNoSimulatedSubfolders) {
      return [];
    }
    throw new DirectoryNotFoundException($"'{path}' does not exist.");
  }

  public ImmutableList<string> GetSubfolderNames(string path) {
    if (SimulatedSubfolderNames.TryGetValue(
          path, out var subfolderNames)) {
      return subfolderNames.ToImmutableList();
    }
    throw new DirectoryNotFoundException($"'{path}' does not exist.");
  }
}