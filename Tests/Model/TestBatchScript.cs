using System.Diagnostics.CodeAnalysis;
using FalconProgrammer.Model;
using JetBrains.Annotations;

namespace FalconProgrammer.Tests.Model;

public class TestBatchScript : BatchScript {
  public TestBatchScript() {
    AppDataFolderName = SettingsTestHelper.TestAppDataFolderName;
    FileSystemService = MockFileSystemService = new MockFileSystemService();
    Serialiser = MockSerialiser = new MockSerialiser();
  }

  [ExcludeFromCodeCoverage]
  [PublicAPI]
  internal MockFileSystemService MockFileSystemService { get; }

  internal MockSerialiser MockSerialiser { get; }
}