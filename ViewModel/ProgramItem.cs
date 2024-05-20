using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public partial class ProgramItem : SoundBankCategory {
  /// <summary>
  ///   Generates <see cref="Program" /> property.
  /// </summary>
  [ObservableProperty] private string _program = string.Empty;

  public ProgramItem(Settings settings, IFileSystemService fileSystemService,
    bool isAdditionItem, bool allowAll) : 
    base(settings, fileSystemService, isAdditionItem, allowAll) { }

  public ObservableCollection<string> Programs { get; } = [];

  protected override void OnCategoryChanged1(string value) {
    PopulatePrograms();
    if (AllowAll) {
      Program = AllCaption;
    }
  }

  private void PopulatePrograms() {
    Programs.Clear();
    if (AllowAll) {
      Programs.Add(AllCaption);
      if (Category == AllCaption) {
        return;
      }
    }
    string categoryFolderPath = Path.Combine(
      Settings.ProgramsFolder.Path, SoundBank, Category);
    var programNames =
      from programPath in FileSystemService.Folder.GetFilePaths(
        categoryFolderPath, "*.uvip")
      select Path.GetFileNameWithoutExtension(programPath);
    foreach (string programName in programNames) {
      Programs.Add(programName);
    }
  }
}