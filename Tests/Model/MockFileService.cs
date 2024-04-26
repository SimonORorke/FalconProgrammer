using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockFileService : IFileService {
  internal bool ExpectedExists { get; set; } = true;
  internal List<string> ExistingPaths { get; } = [];

  public bool Exists(string path) {
    return ExistingPaths.Count == 0 ? ExpectedExists : ExistingPaths.Contains(path);
  }
}