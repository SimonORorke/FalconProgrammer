using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockFileSystemService : IFileSystemService {
  internal bool ExpectedFileExists { get; set; } = true;
  internal bool ExpectedFolderExists { get; set; } = true;
  
  public void CreateFolder(string path) {
  }

  public bool FileExists(string path) {
    return ExpectedFileExists;
  }

  public bool FolderExists(string path) {
    return ExpectedFolderExists;
  }
}