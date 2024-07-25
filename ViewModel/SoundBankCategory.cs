using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FalconProgrammer.Model;
using FalconProgrammer.Model.Options;

namespace FalconProgrammer.ViewModel;

public partial class SoundBankCategory : SoundBankItem {
  internal const string SoundBankErrorMessage =
    "Sound bank folder does not exist or has no subfolders.";

  /// <summary>
  ///   Generates <see cref="Category" /> property.
  /// </summary>
  [ObservableProperty] private string _category = string.Empty;

  public SoundBankCategory(
    Settings settings,
    IFileSystemService fileSystemService,
    bool isAdditionItem, bool allowAll) : base(
    settings, fileSystemService, isAdditionItem) {
    AllowAll = allowAll;
  }

  protected bool AllowAll { get; }
  internal bool IsForAllCategories => Category == AllCaption;
  public ObservableCollection<string> Categories { get; } = [];

  partial void OnCategoryChanged(string value) {
    OnCategoryChanged1(value);
  }

  /// <summary>
  ///   Because the generated OnCategoryChanged method is private
  /// </summary>
  protected virtual void OnCategoryChanged1(string value) { }

  protected override void OnSoundBankChanged1(string value) {
    // On addition after removal, the new sound bank is null. (Or it was at one stage.)
    // This fixes it.
    if (!string.IsNullOrWhiteSpace(value)) {
      PopulateCategories();
      if (AllowAll) {
        Category = AllCaption;
      }
    }
  }

  private void PopulateCategories() {
    Categories.Clear();
    if (AllowAll) {
      Categories.Add(AllCaption);
      if (SoundBank == AllCaption) {
        return;
      }
    }
    try {
      string soundBankFolderPath = Path.Combine(Settings.ProgramsFolder.Path, SoundBank);
      var categoryFolderNames =
        FileSystemService.Folder.GetSubfolderNames(soundBankFolderPath);
      foreach (string categoryFolderName in categoryFolderNames) {
        Categories.Add(categoryFolderName);
      }
    } catch (DirectoryNotFoundException exception) {
      // A sound bank folder in Settings.MustUseGuiScriptProcessorCategories
      // does not exist or contains no category subfolders.
      Console.WriteLine(exception.Message);
    }
  }
}