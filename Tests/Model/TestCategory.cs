﻿using FalconProgrammer.Model;
using FalconProgrammer.Model.XmlLinq;

namespace FalconProgrammer.Tests.Model;

public class TestCategory : Category {
  private MockFileSystemService? _mockFileSystemService;

  public TestCategory(string soundBankFolderPath, string name, Settings settings) :
    base(soundBankFolderPath, name, settings) { }

  /// <summary>
  ///   <see cref="CreateTemplateXml" /> will read the embedded resource file
  ///   with this name in the Tests assembly, ignoring its inputPath parameter.
  /// </summary>
  internal string EmbeddedTemplateFileName { get; set; } = "NoGuiScriptProcessor.xml";

  internal MockFileSystemService MockFileSystemService =>
    _mockFileSystemService ??= new MockFileSystemService();

  protected override IFileSystemService FileSystemService => MockFileSystemService;

  protected override ProgramXml CreateTemplateXml() {
    return new TestProgramXml(this) {
      EmbeddedProgramFileName = EmbeddedTemplateFileName,
      EmbeddedTemplateFileName = EmbeddedTemplateFileName
    };
  }

  internal void ConfigureMockFileSystemService(
    string templateSubfolderPath, string templateProgramFileName) {
    string templateFolderPath = System.IO.Path.Combine(
      Settings.TemplateProgramsFolder.Path,
      templateSubfolderPath);
    string templateProgramPath = System.IO.Path.Combine(
      templateFolderPath, templateProgramFileName);
    string templateParentFolderPath = Directory.GetParent(templateFolderPath)!.FullName;
    string templateFolderName = System.IO.Path.GetFileName(templateFolderPath);
    MockFileSystemService.Folder.ExistingPaths.Add(Settings.TemplateProgramsFolder.Path);
    MockFileSystemService.Folder.ExistingPaths.Add(Path);
    MockFileSystemService.Folder.ExistingPaths.Add(templateFolderPath);
    MockFileSystemService.Folder.SimulatedSubfolderNames.Add(
      templateParentFolderPath, [templateFolderName]);
    MockFileSystemService.Folder.SimulatedFilePaths.Add(
      templateFolderPath, [templateProgramPath]);
  }
}