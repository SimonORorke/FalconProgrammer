using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;

namespace FalconProgrammer.ViewModel;

public partial class BatchScope : SoundBankCategory {
  [ObservableProperty]
  private string _program = string.Empty; // Generates Program property

  public BatchScope(Settings settings, IFileSystemService fileSystemService) : base(
    settings, fileSystemService, false) { }

  public ObservableCollection<string> Programs { get; } = [];

  protected override void OnCategoryChanged1(string value) {
    PopulatePrograms();
    Program = AllCaption;
  }

  private void PopulatePrograms() {
    Programs.Clear();
    Programs.Add(AllCaption);
    if (Category == AllCaption) {
      return;
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