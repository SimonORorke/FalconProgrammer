using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockFileService : IFileService {
  internal List<string> ExistingPaths { get; } = [];
  internal bool SimulatedExists { get; set; } = true;

  public bool Exists(string path) {
    return ExistingPaths.Count == 0 ? SimulatedExists : ExistingPaths.Contains(path);
  }
}