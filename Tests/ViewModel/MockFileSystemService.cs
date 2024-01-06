using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class MockFileSystemService : IFileSystemService {
  internal bool ExpectedFileExists { get; set; } = true;
  internal bool ExpectedFolderExists { get; set; } = true;
  public string AppDataFolderPathMaui { get; set; } = string.Empty;

  public bool FileExists(string path) {
    return ExpectedFileExists;
  }

  public bool FolderExists(string path) {
    return ExpectedFolderExists;
  }
}