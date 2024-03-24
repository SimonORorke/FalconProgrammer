using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.ViewModel;

namespace FalconProgrammer.Tests.ViewModel;

[ExcludeFromCodeCoverage]
public class MockAppDataFolderService : IAppDataFolderService {
  public string AppDataFolderPathMaui { get; set; } = @"C:\MockAppData";
}