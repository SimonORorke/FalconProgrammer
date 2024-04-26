using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockFileService : IFileService {
  internal List<string> ExistingPaths { get; } = [];
  internal bool ExpectedExists { get; set; } = true;

  public bool Exists(string path) {
    return ExistingPaths.Count == 0 ? ExpectedExists : ExistingPaths.Contains(path);
  }
}