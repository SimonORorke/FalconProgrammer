using FalconProgrammer.Model;

namespace FalconProgrammer.Tests.Model;

public class MockFileSystemService : IFileSystemService {
  public MockFileService File => (MockFileService)((IFileSystemService)this).File;
  public MockFolderService Folder => (MockFolderService)((IFileSystemService)this).Folder;
  IFileService IFileSystemService.File { get; } = new MockFileService();
  IFolderService IFileSystemService.Folder { get; } = new MockFolderService();
}