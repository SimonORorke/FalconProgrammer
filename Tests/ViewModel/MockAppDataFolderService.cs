using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

public class MockAppDataFolderService : IAppDataFolderService {
  public string AppDataFolderPathMaui { get; set; } = @"C:\MockAppData";
}