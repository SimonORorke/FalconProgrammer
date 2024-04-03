﻿using FalconProgrammer.Model;
using FalconProgrammer.Model.XmlLinq;

namespace FalconProgrammer.Tests.Model;

public class TestCategory(DirectoryInfo soundBankFolder, string name, Settings settings)
  : Category(soundBankFolder, name, settings) {
  private MockFileSystemService? _mockFileSystemService;
  internal string CategoryFolderPath { get; private set; }

  internal MockFileSystemService MockFileSystemService =>
    _mockFileSystemService ??= new MockFileSystemService();

  protected override IFileSystemService FileSystemService => MockFileSystemService;

  internal void ConfigureMockFileSystemService(
    string templateSubfolderPath, string templateProgramFileName) {
    CategoryFolderPath = System.IO.Path.Combine(
      Settings.ProgramsFolder.Path, SoundBankFolder.Name, Name);
    string templateFolderPath = System.IO.Path.Combine(
      Settings.TemplateProgramsFolder.Path,
      templateSubfolderPath);
    string templateProgramPath = System.IO.Path.Combine(
      templateFolderPath, templateProgramFileName);
    string templateParentFolderPath = Directory.GetParent(templateFolderPath)!.FullName;
    string templateFolderName = System.IO.Path.GetFileName(templateFolderPath);
    MockFileSystemService.ExistingFolderPaths.Add(CategoryFolderPath);
    MockFileSystemService.ExistingFolderPaths.Add(templateFolderPath);
    MockFileSystemService.ExpectedSubfolderNames.Add(
      templateParentFolderPath, [templateFolderName]);
    MockFileSystemService.ExpectedPathsOfFilesInFolder.Add(
      templateFolderPath, [templateProgramPath]);
  }

  protected override ScriptProcessor? GetTemplateScriptProcessor() {
    return null;
  }
}