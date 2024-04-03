using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockFileService : IFileService {
  internal bool ExpectedExists { get; set; } = true;

  public bool Exists(string path) {
    return ExpectedExists;
  }
}